using System.Collections;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Reflection;

namespace THNETII.Utils.GhMultiSync
{
    public class CommandDefinition
    {
        public CommandDefinition(ICommandHandler commandHandler)
        {
            var rootCommand = CreateRootCommand(commandHandler);

            var fileArgument = CreateFileArgument();
            rootCommand.AddArgument(fileArgument);

            var tokenOption = CreateTokenOption();
            rootCommand.AddOption(tokenOption);

            RootCommand = rootCommand;
            FileArgument = fileArgument;
            TokenOption = tokenOption;
        }

        public RootCommand RootCommand { get; }
        public Argument<string[]> FileArgument { get; }
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

        private static Argument<string[]> CreateFileArgument()
        {
            const string file = nameof(file);
            var arg = new Argument<string[]>(file.ToUpperInvariant())
            {
                Description = "Multi-sync specs to execute. Omit or '-': <STDIN>",
                Arity = ArgumentArity.ZeroOrMore
            };
            arg.AddValidator(symbol => symbol.Tokens.Select(t => t.Value)
                .Where(p => p != "-" && !File.Exists(p))
                .Select(ValidationMessages.Instance.FileDoesNotExist)
                .FirstOrDefault()
                );
            //arg.AddSuggestionSource(SuggestFileSystemEntries);

            return arg;
        }

        private static IEnumerable<string> SuggestFileSystemEntries(string pathToMatch)
        {
            if (string.IsNullOrEmpty(pathToMatch))
                return SuggestFileSystemEntries(".");

            string directoryName, searchPattern;
            if (Directory.Exists(pathToMatch))
                (directoryName, searchPattern) = (pathToMatch, "*");
            else
            {
                directoryName = Path.GetDirectoryName(pathToMatch);
                if (string.IsNullOrEmpty(directoryName))
                    directoryName = ".";
                searchPattern = Path.GetFileName(pathToMatch) + "*";
            }

            return Directory.EnumerateFileSystemEntries(directoryName, searchPattern, SearchOption.TopDirectoryOnly)
                .Select(path => Directory.Exists(path) ? path + Path.DirectorySeparatorChar : path);
        }
    }
}
