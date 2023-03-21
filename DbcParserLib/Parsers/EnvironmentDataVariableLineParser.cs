using System.Text.RegularExpressions;

namespace DbcParserLib.Parsers
{
    internal class EnvironmentDataVariableLineParser : ILineParser
    {
        private const string EnvironmentDataVariableLineStarter = "ENVVAR_DATA_ ";
        private const string EnvironmentDataVariableParsingRegex = @"ENVVAR_DATA_\s+([a-zA-Z_][\w]*)\s*:\s+(\d+)\s*;";

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var cleanLine = line.Trim(' ');

            if (cleanLine.StartsWith(EnvironmentDataVariableLineStarter) == false)
                return false;

            var match = Regex.Match(cleanLine, EnvironmentDataVariableParsingRegex);
            if (match.Success)
            {
                builder.AddEnvironmentDataVariable(match.Groups[1].Value, uint.Parse(match.Groups[2].Value));
            }
            return true;
        }
    }
}