using DbcParserLib.Model;
using System;
using System.Globalization;
using System.Linq;

namespace DbcParserLib.Parsers
{
    public class PropertiesLineParser : ILineParser
    {
        private const string PropertiesLineStarter = "BA_ ";

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