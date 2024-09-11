using System.Text.RegularExpressions;
using DbcParserLib.Observers;

namespace DbcParserLib.Parsers
{
    internal class ValueTableDefinitionLineParser : ILineParser
    {
        private const string ValTableGroup = "ValTableName";
        private const string ValueDescriptionGroup = "ValueDescription";
        private const string EndValeDescriptionGroup = "ValueDescriptionEnd";
        private const string ValueTableDefinitionLineStarter = "VAL_TABLE_ ";

        private readonly string m_valueTableDefinitionParsingRegex = $@"VAL_TABLE_\s+(?<{ValTableGroup}>[a-zA-Z_][\w]*)\s+" +
                                                                     $@"(?<{ValueDescriptionGroup}>(?:\d+\s+(?:""[^""]*"")\s+)*)(?<{EndValeDescriptionGroup}>(?:\d+\s+(?:""[^""]*"")\s*));";

        private readonly IParseFailureObserver m_observer;

        public ValueTableDefinitionLineParser(IParseFailureObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var cleanLine = line.Trim(' ');

            if (cleanLine.StartsWith(ValueTableDefinitionLineStarter) == false)
            {
                return false;
            }

            var match = Regex.Match(cleanLine, m_valueTableDefinitionParsingRegex);
            if (match.Success)
            {
                var dictionary = string.IsNullOrEmpty(match.Groups[ValueDescriptionGroup].Value)
                    ? match.Groups[EndValeDescriptionGroup].Value
                    : string.Concat(match.Groups[ValueDescriptionGroup].Value, match.Groups[EndValeDescriptionGroup].Value);

                if (!string.IsNullOrEmpty(dictionary) && dictionary.TryParseToDict(out var valueTableDictionary))
                {
                    builder.AddNamedValueTable(match.Groups[ValTableGroup].Value, valueTableDictionary);
                }
                else
                {
                    m_observer.ValueTableDefinitionSyntaxError();
                }
            }
            else
            {
                m_observer.ValueTableDefinitionSyntaxError();
            }
                
            return true;
        }
    }
}