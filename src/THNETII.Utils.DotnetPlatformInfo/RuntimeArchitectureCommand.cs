using System;
using System.CommandLine;
using System.CommandLine.Invocation;

using Microsoft.DotNet.PlatformAbstractions;

namespace THNETII.Utils.DotnetPlatformInfo
{
    internal static class RuntimeArchitectureCommand
    {
        private static readonly ICommandHandler handler = CommandHandler.Create(() =>
        {
            Console.WriteLine(RuntimeEnvironment.RuntimeArchitecture);
        });

        public static Command Instance { get; } = new Command("arch")
        {
            Handler = handler,
            Description = "Shows the Runtime Process Architecture",
        };
    }
}
