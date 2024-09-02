using System.Text.RegularExpressions;
using DbcParserLib.Observers;

namespace DbcParserLib.Parsers
{
    internal class EnvironmentDataVariableLineParser : ILineParser
    {
        private const string EnvironmentDataVariableLineStarter = "ENVVAR_DATA_ ";
        private const string EnvironmentDataVariableParsingRegex = @"ENVVAR_DATA_\s+([a-zA-Z_][\w]*)\s*:\s+(\d+)\s*;";

        private readonly IParseFailureObserver m_observer;

        public EnvironmentDataVariableLineParser(IParseFailureObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var cleanLine = line.ReplaceNewlinesWithSpace().Trim();

            if (cleanLine.StartsWith(EnvironmentDataVariableLineStarter) == false)
                return false;

            var match = Regex.Match(cleanLine, EnvironmentDataVariableParsingRegex);
            if (match.Success)
                builder.AddEnvironmentDataVariable(match.Groups[1].Value, uint.Parse(match.Groups[2].Value));
            else
                m_observer.EnvironmentDataVariableSyntaxError();
    
            return true;
        }
    }
}