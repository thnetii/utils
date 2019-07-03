using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;

namespace THNETII.Utils.GhMultiSync
{
    public class CommandDefinition
    {
        public CommandDefinition(ICommandHandler commandHandler)
        {
            var rootCommand = CreateRootCommand(commandHandler);

            var tokenOption = CreateTokenOption();
            rootCommand.AddOption(tokenOption);

            RootCommand = rootCommand;
            TokenOption = tokenOption;
        }

        public RootCommand RootCommand { get; }
        public Option TokenOption { get; }

        private static string GetDescription() => typeof(Program).Assembly
            .GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;

        private static RootCommand CreateRootCommand(ICommandHandler commandHandler)
            => new RootCommand(GetDescription()) { Handler = commandHandler };

        private static Option CreateTokenOption()
        {
            const string token = nameof(token);
            var opt = new Option("--" + token)
            {
                Name = token,
                Description = "GitHub access token",
                Argument = new Argument(token.ToUpperInvariant())
                {
                    Description = "OAuth access token or PAT",
                    Arity = ArgumentArity.ZeroOrOne
                }
            };
            opt.AddAlias("-t");

            return opt;
        }
    }
}
