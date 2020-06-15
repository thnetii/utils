using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;

namespace THNETII.Utils.GhMultiSync
{
    public class CommandArguments
    {
        public CommandArguments(CommandDefinition definition, ParseResult parseResult)
        {
            if (definition is null)
                throw new ArgumentNullException(nameof(definition));
            if (parseResult is null)
                throw new ArgumentNullException(nameof(parseResult));

            Token = parseResult.FindResultFor(definition.TokenOption)?
                .GetValueOrDefault<string>();

            FileReaders = parseResult.FindResultFor(definition.FileArgument)?
                .GetValueOrDefault<string[]>()?
                .Distinct(DashStringDistincEqualityComparer.Instance)
                .Select<string, (string, Func<TextReader>)>(p =>
                {
                    if (p == "-")
                        return ("<STDIN>", GetConsoleInTextReader);
                    else
                        return (p, () => File.OpenText(p));
                })
                .ToList();
            if (FileReaders.Count == 0)
                FileReaders.Add(("-", GetConsoleInTextReader));

            static TextReader GetConsoleInTextReader() => Console.In;
        }

        public CommandDefinition Definition { get; }
        public ParseResult ParseResult { get; }
        public string Token { get; set; }
        public List<(string path, Func<TextReader> factory)> FileReaders { get; }

        private class DashStringDistincEqualityComparer : IEqualityComparer<string>
        {
            public static DashStringDistincEqualityComparer Instance { get; } =
                new DashStringDistincEqualityComparer();

            private DashStringDistincEqualityComparer() { }

            public bool Equals(string x, string y) => (x is "-" && y is "-");
            public int GetHashCode(string obj) => obj?.GetHashCode(StringComparison.Ordinal) ?? 0;
        }
    }
}
