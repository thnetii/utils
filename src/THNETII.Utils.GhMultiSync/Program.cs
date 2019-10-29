using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Octokit;
using System;
using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace THNETII.Utils.GhMultiSync
{
    public static partial class Program
    {
        public static Task<int> Main(string[] args)
        {
            var definition = new CommandDefinition(Handler);
            var parser = new CommandLineBuilder(definition.RootCommand)
                .UseDefaults()
                .UseHost(Host.CreateDefaultBuilder, host =>
                {
                    host.ConfigureServices(s => s.AddSingleton(definition));
                    ConfigureHost(host);
                })
                .Build();
            return parser.InvokeAsync(args);
        }

        private static void ConfigureHost(IHostBuilder host)
        {
            host.ConfigureServices(ConfigureServices);
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddProductHeaderValue();
            services.AddOptions<InvocationLifetimeOptions>()
                .Configure<IConfiguration>((opt, config) => config.Bind("Lifetime", opt))
                ;
            services.AddProgramMemoryCache();
            services.AddGitHubClient();
        }

        private static void AddGitHubClient(this IServiceCollection services)
        {
            services.AddOptions<GitHubClientOptions>()
                .Configure<IConfiguration>((opts, config) =>
                {
                    config.Bind(GitHubClientOptions.ConfigurationSectionName, opts);
                })
                .Configure<CommandDefinition, BindingContext>((opts, def, cmdLine) =>
                {
                    var modelBinder = new ModelBinder<GitHubClientOptions>();
                    modelBinder.BindMemberFromOption(o => o.AccessToken, def.TokenOption);
                    modelBinder.UpdateInstance(opts, cmdLine);
                })
                ;
            services.AddTransient(serviceProvider =>
            {
                var githubOptions = serviceProvider
                    .GetRequiredService<IOptions<GitHubClientOptions>>()
                    .Value;
                bool hasAuthType = Enum.TryParse(githubOptions.AuthenticationType,
                    out AuthenticationType authType);
                return githubOptions switch
                {
                    { Login: string login, Password: string password } => hasAuthType
                        ? new Credentials(login, password, authType)
                        : new Credentials(login, password),
                    { AccessToken: string token } => hasAuthType
                        ? new Credentials(token, authType)
                        : new Credentials(token),
                    _ => Credentials.Anonymous,
                };
            });
            services.AddScoped<IConnection>(serviceProvider =>
            {
                var productHeader = serviceProvider.GetRequiredService<ProductHeaderValue>();
                return new Connection(productHeader)
                {
                    Credentials = serviceProvider.GetRequiredService<Credentials>()
                };
            });
            services.AddScoped(serviceProvider =>
            {
                var connection = serviceProvider.GetRequiredService<IConnection>();
                return new GitHubClient(connection);
            });
            services.AddScoped<IGitHubClient, GitHubClient>();
        }

        private static void AddProductHeaderValue(this IServiceCollection services)
        {
            var assembly = Assembly.GetEntryAssembly()
                ?? typeof(Program).Assembly;
            var name = assembly.GetName();
            var version = assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;
            version ??= name.Version?.ToString();
            services.AddSingleton(string.IsNullOrWhiteSpace(version)
                ? new ProductHeaderValue(name.Name)
                : new ProductHeaderValue(name.Name, version)
                );
        }

        private static void AddProgramMemoryCache(this IServiceCollection services)
        {
            services.AddMemoryCache(opt =>
            {
                opt.SizeLimit = 200L * 1024L * 1024L; // 200 MiB
            });
            services.AddOptions<MemoryCacheOptions>()
                .Configure<IConfiguration>((opt, config) =>
                {
                    var path = ConfigurationPath.Combine(
                        nameof(Microsoft.Extensions.Caching),
                        nameof(Microsoft.Extensions.Caching.Memory)
                        );
                    config.Bind(path, opt);
                })
                ;
        }
    }
}
