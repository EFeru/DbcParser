using DbcParserLib.Model;
using System;
using System.Globalization;
using System.Linq;

namespace DbcParserLib.Parsers
{
    public class PropertiesLineParser : ILineParser
    {
        private const string PropertiesLineStarter = "BA_ ";
        private const string PropertiesDefinitionLineStarter = "BA_DEF_ ";
        private const string PropertiesDefinitionDefaultLineStarter = "BA_DEF_DEF_ ";
        private const string PropertyParsingRegex = @"BA_\s+([a-zA-Z_][\w]*)\s+(?:(-?\d+|[0-9.]+)|((BU_|EV_)\s+([a-zA-Z_][\w]*)\s+(?:(-?\d+|[0-9.]+)|""([^""]*)""))|((BO_)\s+(\d+)\s+(?:(-?\d+|[0-9.]+)|""([^""]*)""))|((SG_)\s+(\d+)\s+([a-zA-Z_][\w]*)\s+(?:(-?\d+|[0-9.]+)|""([^""]*)"")))\s*;";
        private const string PropertyDefinitionParsingRegex = @"BA_DEF_\s+(?:(BU_|BO_|SG_)\s+)([a-zA-Z_][\w]*)\s+(?:(?:(INT|HEX)\s+(-?\d+)\s+(-?\d+))|(?:(FLOAT)\s+([0-9].+)\s+([0-9.]+))|(STRING)|(?:(ENUM)\s+""([^""]*)""))\s*;";
        private const string PropertyDefinitionDefaultParsingRegex = @"BA_DEF_DEF_\s+([a-zA-Z_][\w]*)\s+(?:(-?\d+|[0-9.]+)|""([^""]*)"")\s*;";

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var cleanLine = line.Trim(' ', ';');

            if (cleanLine.StartsWith(PropertiesLineStarter) == false)
                return false;

            if (cleanLine.StartsWith("BA_ \"GenMsgCycleTime\" BO_"))
            {
                SetMessageCycleTime(cleanLine, builder);
                return true;
            }

            if (cleanLine.StartsWith("BA_ \"GenSigStartValue\" SG_"))
            {
                SetSignalInitialValue(cleanLine, builder);
                return true;
            }

            return false;
        }

        private static void SetMessageCycleTime(string msgCycleTimeStr, IDbcBuilder builder)
        {
            string[] records = msgCycleTimeStr.SplitBySpace();
            if (records.Length > 4 && uint.TryParse(records[3], out var messageId))
            {
                builder.AddMessageCycleTime(messageId, int.Parse(records[4], CultureInfo.InvariantCulture));
            }
        }

        private static void SetSignalInitialValue(string sigInitialValueStr, IDbcBuilder builder)
        {
            string[] records = sigInitialValueStr.SplitBySpace();
            if (records.Length > 4 && uint.TryParse(records[3], out var messageId))
            {
                builder.AddSignalInitialValue(messageId, records[4], double.Parse(records[5], CultureInfo.InvariantCulture));
            }
        }
    }
}