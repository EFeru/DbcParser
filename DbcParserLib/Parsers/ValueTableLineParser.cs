using System;
using System.Text.RegularExpressions;
using DbcParserLib.Observers;

namespace DbcParserLib.Parsers
{
    internal class ValueTableLineParser : ILineParser
    {
        private const string ValueTableLineStarter = "VAL_ ";
        private const string ValueTableLinkParsingRegex = @"VAL_\s+(\d+)\s+([a-zA-Z_][\w]*)\s+([a-zA-Z_][\w]*)\s*;";
        private const string ValueTableParsingRegex = @"VAL_\s+(?:(?:(\d+)\s+([a-zA-Z_][\w]*))|([a-zA-Z_][\w]*))\s+((?:(?:-?\d+)\s+(?:""[^""]*"")\s+)*)\s*;";

        private readonly IParseFailureObserver m_observer;

        public ValueTableLineParser(IParseFailureObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var cleanLine = line.Trim(' ');

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
                var valueTable = match.Groups[4].Value.TrimStart().Replace("\" ", "\"" + Environment.NewLine);
                var valueTableDictionary = valueTable.ToDictionary();
                if (match.Groups[3].Value != "")
                    builder.LinkTableValuesToEnvironmentVariable(match.Groups[3].Value, valueTableDictionary);
                else
                    builder.LinkTableValuesToSignal(uint.Parse(match.Groups[1].Value), match.Groups[2].Value, valueTableDictionary, valueTable);
                return true;
            }

            m_observer.ValueTableSyntaxError();
            return true;
        }
    }
}