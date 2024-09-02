using System.Text.RegularExpressions;
using DbcParserLib.Observers;

namespace DbcParserLib.Parsers
{
    internal class ValueTableLineParser : ILineParser
    {
        private const string ValueTableLineStarter = "VAL_ ";
        private const string ValueTableLinkParsingRegex = @"VAL_\s+(\d+)\s+([a-zA-Z_][\w]*)\s+([a-zA-Z_][\w]*)\s*;";
        private const string ValueTableParsingRegex = @"VAL_\s+(?:(?:(\d+)\s+([a-zA-Z_][\w]*))|([a-zA-Z_][\w]*))\s+((?:(?:-?\d+)\s+(?:""[^""]*"")\s+)*)((?:(?:-?\d+)\s+(?:""[^""]*"")\s*));";

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

            var match = Regex.Match(cleanLine, ValueTableLinkParsingRegex);
            if (match.Success)
            {
                builder.LinkNamedTableToSignal(uint.Parse(match.Groups[1].Value), match.Groups[2].Value, match.Groups[3].Value);
                return true;
            }

            match = Regex.Match(cleanLine, ValueTableParsingRegex);
            if (match.Success)
            {
                var dictionary = string.IsNullOrEmpty(match.Groups[4].Value) ? match.Groups[5].Value : string.Concat(match.Groups[4].Value, match.Groups[5].Value);

                if (!string.IsNullOrEmpty(dictionary) && dictionary.TryParseToDict(out var valueTableDictionary))
                {
                    if (match.Groups[3].Value != string.Empty)
                        builder.LinkTableValuesToEnvironmentVariable(match.Groups[3].Value, valueTableDictionary);
                    else
                        builder.LinkTableValuesToSignal(uint.Parse(match.Groups[1].Value), match.Groups[2].Value,
                            valueTableDictionary);
                    return true;
                }
            }

            m_observer.ValueTableSyntaxError();
            return true;
        }
    }
}