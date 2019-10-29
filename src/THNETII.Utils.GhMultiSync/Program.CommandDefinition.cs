using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Reflection;

namespace THNETII.Utils.GhMultiSync
{
    public static partial class Program
    {
        public static string? GetDescription()
        {
            var assembly = Assembly.GetEntryAssembly()
                ?? typeof(Program).Assembly;
            return assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description
                ?? assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
        }

        private class CommandDefinition
        {
            public CommandDefinition(ICommandHandler handler)
            {
                RootCommand = new RootCommand
                {
                    Description = GetDescription(),
                    Handler = handler
                };


                TokenOption = CreateTokenOption();
                RootCommand.AddOption(TokenOption);

                FileArgument = CreateFileArgument();
                RootCommand.AddArgument(FileArgument);
            }

            public RootCommand RootCommand { get; }
            public Option TokenOption { get; }
            public Argument<string[]> FileArgument { get; }

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

                string directoryName;
                string searchPattern;
                if (Directory.Exists(pathToMatch))
                    (directoryName, searchPattern) = (pathToMatch, "*");
                else
                {
                    var dirMatch = Path.GetDirectoryName(pathToMatch);
                    directoryName = string.IsNullOrEmpty(dirMatch) ? "." : dirMatch;
                    searchPattern = Path.GetFileName(pathToMatch) + "*";
                }

                return Directory.EnumerateFileSystemEntries(directoryName, searchPattern, SearchOption.TopDirectoryOnly)
                    .Select(path => Directory.Exists(path) ? path + Path.DirectorySeparatorChar : path);
            }
        }
    }
}
