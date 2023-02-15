using DbcParserLib.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Schema;

namespace DbcParserLib.Parsers
{
    public class ValueTableLineParser : ILineParser
    {
        private const string ValueTableLineStarter = "VAL_";

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            if (line.TrimStart().StartsWith(ValueTableLineStarter) == false)
                return false;

            line = line.Trim(';');
            var records = line
                .Trim(' ', ';')
                .Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (records.Length == 1)
                return false;

            if (line.TrimStart().StartsWith("VAL_TABLE_ "))
            {
                if (records.Length > 2)
                {
                    int idxFrom = line.IndexOf(records[1]) + records[1].Length + 1;
                    int length = line.Length - idxFrom;

                    var valueTable = line.Substring(idxFrom, length);
                    valueTable = valueTable.Replace("\" ", "\"" + Environment.NewLine);

                    var valueTableDictionary = ToDict(valueTable);
                    builder.AddNamedValueTable(records[1], valueTableDictionary, valueTable);
                }

                return true;
            }

            var parsed = false;
            if (line.TrimStart().StartsWith("VAL_ "))
            {
                parsed = true;

                if (uint.TryParse(records[1], out var messageId))
                {
                    if (records.Length == 4)
                    {
                        // Last is a table name
                        builder.LinkNamedTableToSignal(messageId, records[2], records[3]);
                    }
                    else if (records.Length > 4)
                    {
                        int idxFrom = line.IndexOf(records[2]) + records[2].Length + 1;
                        int length = line.Length - idxFrom;

                        var valueTable = line.Substring(idxFrom, length);
                        valueTable = valueTable.Replace("\" ", "\"" + Environment.NewLine);

                        var valueTableDictionary = ToDict(valueTable);
                        builder.LinkTableValuesToSignal(messageId, records[2], valueTableDictionary, valueTable);
                    }
                }
            }

            return parsed;
        }

        //ToDictionary
        private IReadOnlyDictionary<int, string> ToDict(string records)
        {
            var dict = new Dictionary<int, string>();

            if (string.IsNullOrWhiteSpace(records))
                return dict;

            using (var reader = new StringReader(records))
            {
                while (reader.Peek() > -1)
                {
                    // Add duplicated key control and act (eg. strict -> break, warning -> keep going and log, silent-> keep going)
                    var tokens = reader.ReadLine().Split(' ');
                    dict[int.Parse(tokens[0])] = tokens[1];
                }
            }
            return dict;
        }
    }
}