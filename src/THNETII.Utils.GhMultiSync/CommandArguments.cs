using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;

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
        }

        public string Token { get; set; }
    }
}
