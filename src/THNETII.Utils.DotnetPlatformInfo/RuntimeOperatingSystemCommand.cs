using System;
using System.CommandLine;
using System.CommandLine.Invocation;

using Microsoft.DotNet.PlatformAbstractions;

namespace THNETII.Utils.DotnetPlatformInfo
{
    internal static class RuntimeOperatingSystemCommand
    {
        private static readonly Option<bool> platformOption = new Option<bool>(
            new[] { "--platform", "-p" },
            "Output Platform enumeration value"
            )
        { Argument = { Name = "BOOL", Arity = ArgumentArity.ZeroOrOne } };
        private static readonly Option<string> formatOption = new Option<string>(
            new[] { "--format", "-f" },
            "Platform enumeration format. Only valid with --platform"
            )
        { Argument = { Name = "FORMAT", Arity = ArgumentArity.ZeroOrOne } };
        private static readonly ICommandHandler handler = CommandHandler.Create(
        (bool platform, string? format) =>
        {
            string output = platform switch
            {
                true => RuntimeEnvironment.OperatingSystemPlatform.ToString(format),
                false => RuntimeEnvironment.OperatingSystem,
            };
            Console.WriteLine(output);
        });

        public static Command Instance { get; } = CreateCommandInstance();

        private static Command CreateCommandInstance()
        {
            var command = new Command("os")
            {
                Handler = handler,
                Description = "Shows the Runtime Operating System",
            };
            command.AddOption(platformOption);
            command.AddOption(formatOption);
            return command;
        }
    }
}
