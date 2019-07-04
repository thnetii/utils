using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Octokit;

using THNETII.Utils.GhMultiSync.Contexts;
using THNETII.Utils.GhMultiSync.Models;

using YamlDotNet.Serialization;

namespace THNETII.Utils.GhMultiSync
{
    public class CommandExecutor
    {
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

        public Dictionary<string, SourceGroupEvaluationContext> SourceGroupContext { get; } =
            new Dictionary<string, SourceGroupEvaluationContext>(StringComparer.Ordinal);

        public Dictionary<RepositoryReferenceSpec, RepositoryEvaluationContext> RepositoryContext { get; } =
            new Dictionary<RepositoryReferenceSpec, RepositoryEvaluationContext>(RepositoryReferenceComparer.Instance);

        public Task ExecuteAsync(CancellationToken cancelToken)
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
                    ExecuteTargetSpec(targetSpec, specModel);

                    cancelToken.ThrowIfCancellationRequested();
                }

                cancelToken.ThrowIfCancellationRequested();
            }

            return Task.CompletedTask;
        }

        private void ExecuteTargetSpec(TargetSpec targetSpec, MultiSyncSpec syncSpec)
        {
            foreach (var groupName in targetSpec.Groups)
                ExecuteTargetSpecGroup(groupName, targetSpec, syncSpec);
        }

        private void ExecuteTargetSpecGroup(string groupName, TargetSpec targetSpec, MultiSyncSpec syncSpec)
        {
            if (!SourceGroupContext.TryGetValue(groupName, out var groupContext) &&
                !TryCreateSourceGroupContext(groupName, syncSpec, out groupContext))
                return;

            ExecuteTargetSpecGroupContext(groupContext, targetSpec);
        }

        private void ExecuteTargetSpecGroupContext(SourceGroupEvaluationContext groupContext, TargetSpec targetSpec)
        {
            foreach (var inheritedContext in groupContext.Inherits)
                ExecuteTargetSpecGroupContext(inheritedContext, targetSpec);

            foreach (var sourceRepository in groupContext.Repositories)
                ExecuteTargetSpecSourceRepository(sourceRepository, targetSpec);
        }

        private void ExecuteTargetSpecSourceRepository(SourceRepositorySpec repoSpec, TargetSpec targetSpec)
        {
            if (!RepositoryContext.TryGetValue(repoSpec, out var repoContext) &&
                !TryCreateRepositoryContext(repoSpec, out repoContext))
                return;

            if (!RepositoryContext.TryGetValue(targetSpec, out var targetContext) &&
                !TryCreateRepositoryContext(targetSpec, out targetContext))
                return;


        }

        private bool TryCreateSourceGroupContext(string groupName, MultiSyncSpec syncSpec, out SourceGroupEvaluationContext groupContext)
        {
            SourceGroupSpec groupSpec;
            try { groupSpec = syncSpec.Groups[groupName]; }
            catch (KeyNotFoundException keyExcept)
            {
                Logger.LogError(keyExcept, $"Unknown group '{{{nameof(groupName)}}}'", groupName);
                groupContext = null;
                return false;
            }

            groupContext = new SourceGroupEvaluationContext
            {
                Inherits = new List<SourceGroupEvaluationContext>(capacity: groupSpec?.Inherits?.Count ?? 0),
                Repositories = groupSpec.Repositories ?? new List<SourceRepositorySpec>()
            };

            foreach (var inheritedGroupName in (groupSpec?.Inherits ?? Enumerable.Empty<string>()).Distinct(StringComparer.Ordinal))
            {
                if (SourceGroupContext.TryGetValue(inheritedGroupName, out var inheritedContext) ||
                    TryCreateSourceGroupContext(inheritedGroupName, syncSpec, out inheritedContext))
                    groupContext.Inherits.Add(inheritedContext);
            }

            SourceGroupContext[groupName] = groupContext;
            return true;
        }

        private bool TryCreateRepositoryContext(RepositoryReferenceSpec repoSpec, out RepositoryEvaluationContext repoContext)
        {
            repoContext = new RepositoryEvaluationContext
            {

            };

            RepositoryContext[repoSpec] = repoContext;
            return true;
        }
    }
}
