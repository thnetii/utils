using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Octokit;

using THNETII.Utils.GhMultiSync.Models;
using THNETII.Utils.GhMultiSync.Models.Yaml;

using YamlDotNet.Serialization;

namespace THNETII.Utils.GhMultiSync
{
    public class CommandExecutor
    {
        public CommandExecutor(
            CommandArguments arguments,
            GitHubClient gitHubClient,
            IOptions<MemoryCacheOptions> memoryCacheOptions,
            IConfiguration configuration,
            ILogger<CommandExecutor> logger = null)
        {
            Arguments = arguments;
            GitHubClient = gitHubClient;
            Configuration = configuration;
            Logger = logger ?? NullLogger<CommandExecutor>.Instance;

            Deserializer = new DeserializerBuilder()
                .WithTypeConverter(new ConditionSpecTypeConverter(() => Deserializer))
                .IgnoreUnmatchedProperties().Build();

            RepositoryCache = new MemoryCache(memoryCacheOptions);
        }

        public CommandArguments Arguments { get; }
        public GitHubClient GitHubClient { get; }
        public IConfiguration Configuration { get; }
        public ILogger<CommandExecutor> Logger { get; }
        public IDeserializer Deserializer { get; }

        public MemoryCache RepositoryCache { get; }
        public Dictionary<RepositoryReference, RepositoryContentChangeTracker> RepositoryChangeTracker { get; } =
            new Dictionary<RepositoryReference, RepositoryContentChangeTracker>(RepositoryReferenceComparer.Instance);

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
                    cancelToken.ThrowIfCancellationRequested();
                }

                cancelToken.ThrowIfCancellationRequested();
            }
        }

        private Task ExecuteCopyFilePath(string sourcePath,
            PathGroupSpec copySpec, RepositoryReference sourceRepo,
            RepositoryReference targetRepo, CancellationToken cancelToken)
            => ExecuteCopyFilePath(sourcePath, sourcePath, copySpec, sourceRepo, targetRepo, cancelToken);

        private async Task ExecuteCopyFilePath(string sourcePath, string sourceContentPath,
            PathGroupSpec copySpec, RepositoryReference sourceRepo,
            RepositoryReference targetRepo, CancellationToken cancelToken)
        {
            RepositoryContentEntry sourceContentEntry;
            try
            {
                sourceContentEntry = await GetRepositoryContentEntry(sourceRepo, sourceContentPath)
                    .ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (NotFoundException notFoundExcept)
            {
                Logger.LogError(notFoundExcept, $"Unable to get repository contents from {{{nameof(sourceRepo)}}} at path: {{{nameof(sourcePath)}}}", sourceRepo.ToLogString(), sourceContentPath);
                return;
            }
            if (cancelToken.IsCancellationRequested)
                return;
            var sourceContents = sourceContentEntry.Contents;
            foreach (var sourceContent in sourceContents)
            {
                bool unsupportedContentType = !sourceContent.Type.TryParse(out var sourceContentType);
                if (!unsupportedContentType)
                {
                    switch (sourceContentType)
                    {
                        case ContentType.File:
                            await ExecuteCopyFileContent(sourcePath, sourceContent,
                                copySpec, sourceRepo, targetRepo, cancelToken)
                                .ConfigureAwait(continueOnCapturedContext: false);
                            break;
                        case ContentType.Dir:
                            await ExecuteCopyFilePath(sourcePath, sourceContent.Path,
                                copySpec, sourceRepo, targetRepo, cancelToken)
                                .ConfigureAwait(continueOnCapturedContext: false);
                            break;
                        case ContentType.Symlink:
                            await ExecuteCopyFilePath(sourcePath, sourceContent.Target,
                                copySpec, sourceRepo, targetRepo, cancelToken)
                                .ConfigureAwait(continueOnCapturedContext: false);
                            break;
                        default:
                            unsupportedContentType = true;
                            break;
                    }
                }
                if (unsupportedContentType)
                    Logger.LogWarning($"Unsupported content type '{{{nameof(sourceContentType)}}}' in repository {{{nameof(sourceRepo)}}} at path: {{{nameof(sourcePath)}}}", sourceContent.Type, sourceRepo.ToLogString(), sourceContent.Path);
                if (cancelToken.IsCancellationRequested)
                    return;
            }
        }

        private async Task ExecuteCopyFileContent(string sourcePath,
            RepositoryContent sourceContent, PathGroupSpec copySpec,
            RepositoryReference sourceRepo,
            RepositoryReference targetRepo,
            CancellationToken cancelToken)
        {
            var targetPath = await GetTargetPath(sourceContent, copySpec, targetRepo)
                .ConfigureAwait(continueOnCapturedContext: false);

            if (cancelToken.IsCancellationRequested)
                return;

            RepositoryContentEntry targetEntry;
            try
            {
                targetEntry = await GetRepositoryContentEntry(targetRepo, targetPath)
                    .ConfigureAwait(false);
            }
            catch (NotFoundException) { targetEntry = null; }
            var targetContent = targetEntry?.Leaf;

            bool copyCond = copySpec.Condition?.Evaluate(
                sourcePath, sourceRepo, sourceContent,
                targetPath, targetRepo, targetContent
                ) ?? true;

            if (!copyCond)
                Logger.LogDebug($"Not copying path '{{{nameof(sourcePath)}}}' from {{{nameof(sourceRepo)}}} to '{{{nameof(targetPath)}}}' in {{{nameof(targetRepo)}}}. Condition of type {{conditionType}} evaluated {{conditionValue}}.", sourcePath, sourceRepo.ToLogString(), targetPath, targetRepo.ToLogString(), copySpec.Condition?.GetType(), copyCond);


        }

        private async Task<string> GetTargetPath(RepositoryContent sourceContent,
            PathGroupSpec pathSpec, RepositoryReference targetRepo)
        {
            var targetPath = pathSpec.DestinationPath;
            RepositoryContentEntry targetEntry;
            try
            {
                targetEntry = await GetRepositoryContentEntry(targetRepo, targetPath)
                    .ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (NotFoundException) { targetEntry = null; }

            switch (targetEntry)
            {
                case null when pathSpec.SourcePaths.Count > 1:
                case RepositoryContentEntry _
                    when !(targetEntry.Leaf is null) &&
                        targetEntry.Leaf.Type.TryParse(out var targetType) &&
                        targetType == ContentType.Dir:
                case RepositoryContentEntry _ when targetEntry.Leaf is null:
                    return pathSpec.DestinationPath.TrimEnd('/').TrimEnd() +
                        "/" + sourceContent.Name;

                case null:
                    return targetPath;

                case RepositoryContentEntry _
                    when !(targetEntry.Leaf is null):
                    return targetEntry.Leaf.Path;
            }

            return null;
        }

        private async Task<RepositoryContentEntry> GetRepositoryContentEntry(RepositoryReference repoSpec, string path)
        {
            var entryKey = new RepositoryPathReference
            {
                RepositoryOwner = repoSpec.RepositoryOwner,
                RepositoryName = repoSpec.RepositoryName,
                TreeReference = repoSpec.TreeReference,
                Path = path
            };
            var entryObj = await RepositoryCache.GetOrCreateAsync(
                entryKey, CreateCachedRepositoryContentEntry
                ).ConfigureAwait(continueOnCapturedContext: false);
            if (entryObj.Contents is null)
            {
                var entryContents = await DownloadRepositoryContents(repoSpec, path)
                    .ConfigureAwait(continueOnCapturedContext: false);
                AddRepositoryContentsToCache(repoSpec, entryContents);
                entryObj.Contents = entryContents;
            }
            return entryObj;
        }

        private async Task<RepositoryContentEntry> CreateCachedRepositoryContentEntry(ICacheEntry entryOptions)
        {
            var entryKey = (RepositoryPathReference)entryOptions.Key;
            string entryPath = entryKey.Path;
            var entryContents = await DownloadRepositoryContents(entryKey, entryPath)
                .ConfigureAwait(continueOnCapturedContext: false);
            AddRepositoryContentsToCache(entryKey, entryContents);

            if (RepositoryCache.TryGetValue(entryKey, out RepositoryContentEntry entryObj))
                entryObj.Contents = entryContents;
            else
                entryObj = new RepositoryContentEntry { Contents = entryContents };
            entryOptions.Size = entryObj.Leaf?.Size;

            return entryObj;
        }

        private void AddRepositoryContentsToCache(RepositoryReference repoSpec, IEnumerable<RepositoryContent> contentList)
        {
            foreach (var repoContent in contentList)
            {
                var subKey = new RepositoryPathReference
                {
                    RepositoryOwner = repoSpec.RepositoryOwner,
                    RepositoryName = repoSpec.RepositoryName,
                    TreeReference = repoSpec.TreeReference,
                    Path = repoContent.Path
                };
                if (RepositoryCache.TryGetValue(subKey, out RepositoryContentEntry subEntry))
                    subEntry.Leaf = repoContent;
                else
                    subEntry = new RepositoryContentEntry { Leaf = repoContent };
                var subOptions = new MemoryCacheEntryOptions { Size = repoContent.Size };
                RepositoryCache.Set(subKey, subEntry, subOptions);
                if (repoContent.Type.TryParse(out var repoContentType) && repoContentType == ContentType.File)
                    subEntry.Contents = new List<RepositoryContent>(capacity: 1) { repoContent };
            }
        }

        private Task<IReadOnlyList<RepositoryContent>> DownloadRepositoryContents(RepositoryReference repoSpec, string path)
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

        private async Task UploadRepositoryContent(
            RepositoryReference targetRepo, string path,
            string commitMessage, string sourceContentBase64,
            CancellationToken cancelToken)
        {
            var changeTracker = GetOrCreateRepositoryChangeTracker(targetRepo);
            await changeTracker.Lock.WaitAsync(cancelToken).ConfigureAwait(false);
            try
            {
                await UploadRepositoryContent(changeTracker, path, commitMessage,
                    sourceContentBase64, cancelToken
                    ).ConfigureAwait(continueOnCapturedContext: false);
            }
            finally { changeTracker.Lock.Release(); }
        }

        private async Task UploadRepositoryContent(
            RepositoryContentChangeTracker changeTracker, string path,
            string commitMessage, string sourceContentBase64,
            CancellationToken cancelToken)
        {
            RepositoryContentEntry targetEntry;
            try
            {
                targetEntry = await GetRepositoryContentEntry(changeTracker.Reference, path)
                    .ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (NotFoundException) { targetEntry = null; }
            if (cancelToken.IsCancellationRequested)
                return;

            var (client, owner, name) = (
                GitHubClient.Repository.Content,
                changeTracker.Reference.RepositoryOwner,
                changeTracker.Reference.RepositoryName
                );
            Task<RepositoryContentChangeSet> changeSetTask;
            if (targetEntry is null)
            {
                var uploadOptions = new CreateFileRequest(
                    commitMessage,
                    sourceContentBase64,
                    null,
                    convertContentToBase64: false
                    );

                changeSetTask = client.CreateFile(
                    owner, name, path, uploadOptions);
            }
            else
            {
                UpdateFileRequest uploadOptions = new UpdateFileRequest(
                    commitMessage,
                    sourceContentBase64,
                    targetEntry.Leaf?.Sha,
                    null,
                    convertContentToBase64: false
                    );
                changeSetTask = client.UpdateFile(
                    owner, name, path, uploadOptions);
            }

            var changeSetResult = await changeSetTask.ConfigureAwait(false);
            changeTracker.Reference = new RepositoryReference
            {
                RepositoryOwner = owner,
                RepositoryName = name,
                TreeReference = changeSetResult.Commit.Sha
            };
        }

        private RepositoryContentChangeTracker GetOrCreateRepositoryChangeTracker(RepositoryReference repo)
        {
            RepositoryContentChangeTracker changeTracker;
            lock (RepositoryChangeTracker)
            {
                if (!RepositoryChangeTracker.TryGetValue(repo, out changeTracker))
                {
                    changeTracker = new RepositoryContentChangeTracker { Reference = repo };
                    RepositoryChangeTracker[repo] = changeTracker;
                }
            }
            return changeTracker;
        }
    }
}
