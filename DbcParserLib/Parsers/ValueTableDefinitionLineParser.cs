﻿using System;
using System.Text.RegularExpressions;

namespace DbcParserLib.Parsers
{
    internal class ValueTableDefinitionLineParser : ILineParser
    {
        private const string ValueTableDefinitionLineStarter = "VAL_TABLE_ ";
        private const string ValueTableDefinitionParsingRegex = @"VAL_TABLE_\s+([a-zA-Z_][\w]*)\s+((?:\d+\s+(?:""[^""]*"")\s+)*)\s*;";

        private readonly IParseObserver m_observer;

        public ValueTableDefinitionLineParser(IParseObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, int lineNumber, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var cleanLine = line.Trim(' ');

            if (cleanLine.StartsWith(ValueTableDefinitionLineStarter) == false)
                return false;

            var match = Regex.Match(cleanLine, ValueTableDefinitionParsingRegex);
            if (match.Success)
            {
                var valueTable = match.Groups[2].Value.Replace("\" ", "\"" + Environment.NewLine);
                var valueTableDictionary = valueTable.ToDictionary();
                builder.AddNamedValueTable(match.Groups[1].Value, valueTableDictionary, valueTable);
            }
            return true;
        }
    }
}