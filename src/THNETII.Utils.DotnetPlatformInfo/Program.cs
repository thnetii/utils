using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Reflection;
using System.Threading.Tasks;

namespace THNETII.Utils.DotnetPlatformInfo
{
    public static class Program
    {
        private static readonly RootCommand rootCommand =
            new RootCommand(GetDescription());

        private static string GetDescription()
        {
            var progAssembly = typeof(Program).Assembly;
            return progAssembly
                .GetCustomAttribute<AssemblyDescriptionAttribute>()?
                .Description
                ??
                progAssembly
                .GetCustomAttribute<AssemblyProductAttribute>()?
                .Product
                ?? string.Empty;
        }

        public static Task<int> Main(string[] args)
        {
            var parser = new CommandLineBuilder(rootCommand)
                .AddCommand(RuntimeOperatingSystemCommand.Instance)
                .AddCommand(RuntimeOperatingSystemVersionCommand.Instance)
                .AddCommand(RuntimeArchitectureCommand.Instance)
                .AddCommand(RuntimeIdentifierCommand.Instance)
                .UseDefaults()
                .Build();

            return parser.InvokeAsync(args ?? Array.Empty<string>());
        }
    }
}
