using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Octokit;

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
        }

        public CommandArguments Arguments { get; }
        public GitHubClient GitHubClient { get; }
        public IConfiguration Configuration { get; }
        public ILogger<CommandExecutor> Logger { get; }

        public Task ExecuteAsync(CancellationToken cancelToken)
        {
            Logger.LogDebug(nameof(ExecuteAsync));

            return Task.CompletedTask;
        }
    }
}
