using System;
using System.CommandLine;
using System.CommandLine.Invocation;

using Microsoft.DotNet.PlatformAbstractions;

namespace THNETII.Utils.DotnetPlatformInfo
{
    internal static class RuntimeIdentifierCommand
    {
        private static readonly ICommandHandler handler = CommandHandler.Create(() =>
        {
            Console.WriteLine(RuntimeEnvironment.GetRuntimeIdentifier());
        });

        public static Command Instance { get; } = new Command("rid")
        {
            Handler = handler,
            Description = "Shows the Runtime Identifier",
        };
    }
}
