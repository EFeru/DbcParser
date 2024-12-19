using System.Text.RegularExpressions;
using DbcParserLib.Observers;

namespace DbcParserLib.Parsers
{
    internal class EnvironmentDataVariableLineParser : ILineParser
    {
        private const string NameGroup = "Name";
        private const string DataSizeGroup = "DataSize";
        private const string EnvironmentDataVariableLineStarter = "ENVVAR_DATA_ ";

        private readonly string m_environmentDataVariableParsingRegex = $@"ENVVAR_DATA_\s+(?<{NameGroup}>[a-zA-Z_][\w]*)\s*:\s+(?<{DataSizeGroup}>\d+)\s*;";

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

            var match = Regex.Match(cleanLine, m_environmentDataVariableParsingRegex);
            if (match.Success)
                builder.AddEnvironmentDataVariable(match.Groups[NameGroup].Value, uint.Parse(match.Groups[DataSizeGroup].Value));
            else
                m_observer.EnvironmentDataVariableSyntaxError();

            return true;
        }
    }
}