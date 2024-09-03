using System.Text.RegularExpressions;
using DbcParserLib.Observers;

namespace DbcParserLib.Parsers
{
    internal class ValueTableLineParser : ILineParser
    {
        private const string MessageIdGroup = "MessageId";
        private const string SignalNameGroup = "SignalName";
        private const string ValueTableGroup = "ValueTable";
        private const string EnvVarGroup = "EnvVarName";
        private const string ValueDescriptionGroup = "ValueDescription";
        private const string EndValeDescriptionGroup = "ValueDescriptionEnd";
        private const string ValueTableLineStarter = "VAL_ ";

        private readonly string m_valueTableLinkParsingRegex = $@"VAL_\s+(?<{MessageIdGroup}>\d+)\s+(?<{SignalNameGroup}>[a-zA-Z_][\w]*)\s+(?<{ValueTableGroup}>[a-zA-Z_][\w]*)\s*;";
        private readonly string m_valueTableParsingRegex = $@"VAL_\s+(?:(?:(?<{MessageIdGroup}>\d+)\s+(?<{SignalNameGroup}>[a-zA-Z_][\w]*))|(?<{EnvVarGroup}>[a-zA-Z_][\w]*))\s+" +
                                                           $@"(?<{ValueDescriptionGroup}>(?:(?:-?\d+)\s+(?:""[^""]*"")\s+)*)(?<{EndValeDescriptionGroup}>(?:(?:-?\d+)\s+(?:""[^""]*"")\s*));";

        private readonly IParseFailureObserver m_observer;

        public ValueTableLineParser(IParseFailureObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var cleanLine = line.ReplaceNewlinesWithSpace().Trim();

            if (cleanLine.StartsWith(ValueTableLineStarter) == false)
                return false;

            var match = Regex.Match(cleanLine, m_valueTableLinkParsingRegex);
            if (match.Success)
            {
                builder.LinkNamedTableToSignal(uint.Parse(match.Groups[MessageIdGroup].Value), match.Groups[SignalNameGroup].Value, match.Groups[ValueTableGroup].Value);
                return true;
            }

            match = Regex.Match(cleanLine, m_valueTableParsingRegex);
            if (match.Success)
            {
                var dictionary = string.IsNullOrEmpty(match.Groups[ValueDescriptionGroup].Value)
                    ? match.Groups[EndValeDescriptionGroup].Value
                    : string.Concat(match.Groups[ValueDescriptionGroup].Value, match.Groups[EndValeDescriptionGroup].Value);

                if (!string.IsNullOrEmpty(dictionary) && dictionary.TryParseToDict(out var valueTableDictionary))
                {
                    if (match.Groups[EnvVarGroup].Value.Equals(string.Empty) == false)
                        builder.LinkTableValuesToEnvironmentVariable(match.Groups[EnvVarGroup].Value, valueTableDictionary);
                    else
                        builder.LinkTableValuesToSignal(uint.Parse(match.Groups[MessageIdGroup].Value), match.Groups[SignalNameGroup].Value,
                            valueTableDictionary);
                    return true;
                }
            }

            m_observer.ValueTableSyntaxError();
            return true;
        }
    }
}