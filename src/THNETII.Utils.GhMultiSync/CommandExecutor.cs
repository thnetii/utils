using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Octokit;

using THNETII.Utils.GhMultiSync.Models;

using YamlDotNet.Serialization;

namespace THNETII.Utils.GhMultiSync
{
    public class CommandExecutor
    {
        private static readonly Uri rootUri = new Uri(@"cache://github.com/");

        public CommandExecutor(
            CommandArguments arguments,
            GitHubClient gitHubClient,
            IConfiguration configuration,
            ILogger<CommandExecutor> logger = null)
        {
            Arguments = arguments;
            GitHubClient = gitHubClient;
            Configuration = configuration;
            Logger = logger ?? NullLogger<CommandExecutor>.Instance;

            Deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();
        }

        public CommandArguments Arguments { get; }
        public GitHubClient GitHubClient { get; }
        public IConfiguration Configuration { get; }
        public ILogger<CommandExecutor> Logger { get; }
        public IDeserializer Deserializer { get; }

        public Dictionary<RepositoryReferenceSpec, RepositoryCache> RepositoryCache { get; } =
            new Dictionary<RepositoryReferenceSpec, RepositoryCache>(RepositoryReferenceComparer.Instance);

        public async Task ExecuteAsync(CancellationToken cancelToken)
        {
            foreach (var (path, factory) in Arguments.FileReaders)
            {
                Logger.LogDebug($"Deserializing YAML file '{{{nameof(path)}}}'", path);
                MultiSyncSpec specModel;
                using (var textReader = factory())
                    specModel = Deserializer.Deserialize<MultiSyncSpec>(textReader);
                cancelToken.ThrowIfCancellationRequested();

                foreach (var targetSpec in specModel.Targets ?? Enumerable.Empty<TargetSpec>())
                {
                    await ExecuteTargetSpec(targetSpec, specModel)
                        .ConfigureAwait(false);

                    cancelToken.ThrowIfCancellationRequested();
                }

                cancelToken.ThrowIfCancellationRequested();
            }
        }

        private Task ExecuteTargetSpec(TargetSpec targetSpec, MultiSyncSpec syncSpec)
        {
            return Task.WhenAll((targetSpec.Groups ?? Enumerable.Empty<string>())
                .Select(groupName => ExecuteTargetGroupName(groupName, targetSpec, syncSpec))
                );
        }

        private Task ExecuteTargetGroupName(string groupName, TargetSpec targetSpec, MultiSyncSpec syncSpec)
        {
            if (!syncSpec.Groups.TryGetValue(groupName, out var groupSpec))
            {
                Logger.LogError($"Unknown group name specified: {{{nameof(groupName)}}}", groupName);
                return Task.CompletedTask;
            }

            return ExecuteTargetGroupSpec(groupSpec, targetSpec, syncSpec);
        }

        private async Task ExecuteTargetGroupSpec(SourceGroupSpec groupSpec, TargetSpec targetSpec, MultiSyncSpec syncSpec)
        {
            await Task.WhenAll((groupSpec.Inherits ?? Enumerable.Empty<string>())
                .Select(groupName => ExecuteTargetGroupName(groupName, targetSpec, syncSpec))
                ).ConfigureAwait(false);

            await Task.WhenAll((groupSpec.Repositories ?? Enumerable.Empty<SourceRepositorySpec>())
                .Select(sourceRepository => ExecuteTargetSpecSourceRepository(sourceRepository, targetSpec))
                ).ConfigureAwait(false);
        }

        private Task ExecuteTargetSpecSourceRepository(SourceRepositorySpec sourceSpec, TargetSpec targetSpec)
        {
            var sourceCache = GetOrCreateRepositoryCache(sourceSpec);
            var targetCache = GetOrCreateRepositoryCache(targetSpec);

            return Task.WhenAll(
                (sourceSpec.CopyFiles ?? Enumerable.Empty<PathGroupSpec>())
                .Select(fileSpec =>
                {
                    return ExecuteTargetCopyFilesSpec(
                        fileSpec,
                        sourceCache,
                        targetCache,
                        sourceSpec,
                        targetSpec
                        );
                }));
        }

        private Task ExecuteTargetCopyFilesSpec(PathGroupSpec fileSpec, RepositoryCache sourceCache, RepositoryCache targetCache, RepositoryReferenceSpec sourceSpec, RepositoryReferenceSpec targetSpec)
        {
            return Task.WhenAll(
                (fileSpec.SourcePaths ?? Enumerable.Empty<string>())
                .Select(path =>
                {
                    return ExecuteTargetCopyFilePath(
                        path,
                        fileSpec,
                        sourceCache,
                        targetCache,
                        sourceSpec,
                        targetSpec
                        );
                }));
        }

        private async Task ExecuteTargetCopyFilePath(string sourcePath, PathGroupSpec fileSpec, RepositoryCache sourceCache, RepositoryCache targetCache, RepositoryReferenceSpec sourceSpec, RepositoryReferenceSpec targetSpec)
        {
            RepositoryCacheEntry sourceEntry;
            try
            {
                sourceEntry = await GetOrCreateRepositoryCacheEntry(sourceCache, sourcePath, sourceSpec)
                    .ConfigureAwait(false);
            }
            catch (NotFoundException notFoundExcept)
            {
                Logger.LogError(notFoundExcept, $"Unable to copy file from source path: {{{nameof(sourcePath)}}}", sourcePath);
                return;
            }

            if (fileSpec.Condition is ConditionSpec)
            {
                // Return if condition evaluation returns false
            }


        }

        private RepositoryCache GetOrCreateRepositoryCache(RepositoryReferenceSpec repoSpec)
        {
            bool repoFound;
            RepositoryCache repoCache;
            lock (RepositoryCache)
            { repoFound = RepositoryCache.TryGetValue(repoSpec, out repoCache); }
            if (!repoFound)
            {
                repoCache = new RepositoryCache();
                lock (RepositoryCache)
                { RepositoryCache[repoSpec] = repoCache; }
            }
            return repoCache;
        }

        private async Task<RepositoryCacheEntry> GetOrCreateRepositoryCacheEntry(RepositoryCache repoCache, string path, RepositoryReferenceSpec repoSpec)
        {
            bool entryFound;
            RepositoryCacheEntry entryObj;
            var entries = repoCache.Entries;
            lock (entries)
            { entryFound = entries.TryGetValue(path, out entryObj); }
            if (!entryFound)
                entryObj = new RepositoryCacheEntry();
            switch (entryObj)
            {
                case RepositoryCacheEntry _
                when entryObj.Leaf is null
                    && entryObj.Contents is null:
                case RepositoryCacheEntry _
                when entryObj.Leaf is RepositoryContent entryLeaf
                    && entryLeaf.Type.TryParse(out var entryType)
                    && entryType == ContentType.Dir
                    && entryObj.Contents is null:
                    await GetRepositoryContentsForEntry(path, entryObj, repoSpec, entries)
                        .ConfigureAwait(false);
                    break;
            }

            lock (entries)
            {
                if (entries.TryGetValue(path, out var newEntryObj))
                {
                    newEntryObj.Contents = entryObj.Contents;
                    entryObj = newEntryObj;
                }
                else
                    entries[path] = entryObj;
            }
            return entryObj;
        }

        private async Task GetRepositoryContentsForEntry(string path, RepositoryCacheEntry entryObj, RepositoryReferenceSpec repoSpec, Dictionary<string, RepositoryCacheEntry> entries)
        {
            bool entryFound;
            var repoContentList = await GetRepositoryContent(repoSpec, path)
                    .ConfigureAwait(continueOnCapturedContext: false);
            foreach (var repoContentInfo in repoContentList)
            {
                var subPath = repoContentInfo.Path;
                RepositoryCacheEntry subEntry;
                lock (entries)
                { entryFound = entries.TryGetValue(subPath, out subEntry); }
                if (entryFound)
                    subEntry.Leaf = repoContentInfo;
                else
                {
                    subEntry = new RepositoryCacheEntry { Leaf = repoContentInfo };
                    lock (entries)
                    { entries[subPath] = subEntry; }
                }
            }
            entryObj.Contents = repoContentList;
        }

        private Task<IReadOnlyList<RepositoryContent>> GetRepositoryContent(RepositoryReferenceSpec repoSpec, string path)
        {
            var (client, owner, name, @ref) = (
                GitHubClient.Repository.Content,
                repoSpec.RepositoryOwner,
                repoSpec.RepositoryName,
                repoSpec.TreeReference
                );
            if (string.IsNullOrEmpty(@ref))
            {
                if (string.IsNullOrEmpty(path))
                    return client.GetAllContents(owner, name);
                else
                    return client.GetAllContents(owner, name, path);
            }
            else
            {
                if (string.IsNullOrEmpty(path))
                    return client.GetAllContentsByRef(owner, name, @ref);
                else
                    return client.GetAllContentsByRef(owner, name, path, @ref);
            }
        }
    }
}
