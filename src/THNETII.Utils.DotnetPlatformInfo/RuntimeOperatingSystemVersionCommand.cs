using System;
using System.CommandLine;
using System.CommandLine.Invocation;

using Microsoft.DotNet.PlatformAbstractions;

namespace THNETII.Utils.DotnetPlatformInfo
{
    internal static class RuntimeOperatingSystemVersionCommand
    {
        private static readonly ICommandHandler handler = CommandHandler.Create(() =>
        {
            Console.WriteLine(RuntimeEnvironment.OperatingSystemVersion);
        });

        public static Command Instance { get; } = new Command("osversion")
        {
            Handler = handler,
            Description = "Shows the Runtime Operating System version",
        };
    }
}
