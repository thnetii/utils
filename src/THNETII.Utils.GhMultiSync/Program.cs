using System;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Octokit;

namespace THNETII.Utils.GhMultiSync
{
    public static class Program
    {
        private const string GitHubConfigPath = "GitHub";
        private static readonly string AccessTokenConfigPath =
            ConfigurationPath.Combine(GitHubConfigPath, "AccessToken");
        private static readonly string AccessTokenFallbackVariableConfigPath =
            ConfigurationPath.Combine(GitHubConfigPath, "AccessTokenFallbackVariable");

        public static Task<int> Main(string[] args)
        {
            var commandHandler = CommandHandler.Create<IHost, CancellationToken>(RunAsync);
            var commandDefinition = new CommandDefinition(commandHandler);

            var parser = new CommandLineBuilder(commandDefinition.RootCommand)
                .UseDefaults()
                .UseHost(Host.CreateDefaultBuilder, host =>
                {
                    host.ConfigureServices(services => services.AddSingleton(commandDefinition));
                    ConfigureHost(host);
                })
                .Build();

            return parser.InvokeAsync(args);

            async Task RunAsync(IHost host, CancellationToken cancelToken)
            {
                var serviceProvider = host.Services;
                using (var scope = serviceProvider.CreateScope())
                {
                    var scopeProvider = scope.ServiceProvider;
                    var executor = scopeProvider.GetRequiredService<CommandExecutor>();

                    await executor.ExecuteAsync(cancelToken).ConfigureAwait(false);
                }
            }
        }

        private static void ConfigureHost(IHostBuilder host)
            => host.ConfigureServices(ConfigureServices);

        private static void ConfigureServices(HostBuilderContext context,
            IServiceCollection services)
        {
            services.AddSingleton<CommandArguments>();
            services.AddScoped<CommandExecutor>();

            services.AddScoped(serviceProvider => new GitHubClient(serviceProvider
                .GetRequiredService<IConnection>()));
            services.AddSingleton(
                new ProductHeaderValue(
                    typeof(Program).Assembly.GetName().Name,
                    typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                    ?? typeof(Program).Assembly.GetName().Version.ToString()
                    )
                );
            services.AddScoped<IConnection>(serviceProvider =>
            {
                var arguments = serviceProvider.GetRequiredService<CommandArguments>();
                var token = arguments.Token;

                if (string.IsNullOrWhiteSpace(token))
                {
                    var config = serviceProvider.GetRequiredService<IConfiguration>();
                    var path = AccessTokenConfigPath;
                    token = config[path];
                }

                if (string.IsNullOrWhiteSpace(token))
                {
                    var config = serviceProvider.GetRequiredService<IConfiguration>();
                    var path = AccessTokenFallbackVariableConfigPath;
                    var variable = config[path];

                    if (!string.IsNullOrWhiteSpace(variable))
                        token = Environment.GetEnvironmentVariable(variable);
                }

                var productHeader = serviceProvider.GetRequiredService<ProductHeaderValue>();
                Credentials credentials = string.IsNullOrWhiteSpace(token)
                    ? Credentials.Anonymous
                    : new Credentials(token);
                return new Connection(productHeader)
                {
                    Credentials = credentials
                };
            });

            services.AddMemoryCache(options =>
            {
                options.SizeLimit = 200L * 1024L * 1024L; // 200 MiB
            });

            services.AddSingleton<IPostConfigureOptions<InvocationLifetimeOptions>, InvocationLifetimeOptionsPostConfigure>();
            services.AddSingleton<IPostConfigureOptions<MemoryCacheOptions>, MemoryCacheOptionsPostConfigure>();
        }

        [SuppressMessage("Performance", "CA1812: Avoid uninstantiated internal classes", Justification = nameof(Microsoft.Extensions.DependencyInjection))]
        private class InvocationLifetimeOptionsPostConfigure : IPostConfigureOptions<InvocationLifetimeOptions>
        {
            private readonly IConfiguration configuration;

            public InvocationLifetimeOptionsPostConfigure(IConfiguration configuration)
            {
                this.configuration = configuration;
            }

            public void PostConfigure(string name, InvocationLifetimeOptions options)
                => configuration.Bind("Lifetime", options);
        }

        [SuppressMessage("Performance", "CA1812: Avoid uninstantiated internal classes", Justification = nameof(Microsoft.Extensions.DependencyInjection))]
        private class MemoryCacheOptionsPostConfigure : IPostConfigureOptions<MemoryCacheOptions>
        {
            private static readonly string ConfigPath = ConfigurationPath.Combine(
                nameof(Microsoft.Extensions.Caching),
                nameof(Microsoft.Extensions.Caching.Memory)
                );

            private readonly IConfiguration configuration;

            public MemoryCacheOptionsPostConfigure(IConfiguration configuration)
            {
                this.configuration = configuration;
            }

            public void PostConfigure(string name, MemoryCacheOptions options)
                => configuration.Bind(ConfigPath, options);
        }
    }
}
