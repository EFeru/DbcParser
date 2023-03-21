using DbcParserLib.Model;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DbcParserLib.Parsers
{
    internal class PropertiesLineParser : ILineParser
    {
        private const string PropertiesLineStarter = "BA_ ";
        private const string PropertyParsingRegex = @"BA_\s+""([a-zA-Z_][\w]*)""(?:\s+(?:(BU_|EV_)\s+([a-zA-Z_][\w]*))|\s+(?:(BO_)\s+(\d+))|\s+(?:(SG_)\s+(\d+)\s+([a-zA-Z_][\w]*)))?\s+(-?\d+|[\d\+\-eE.]+|""[^""]*"");";

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var cleanLine = line.Trim(' ');

            if (cleanLine.StartsWith(PropertiesLineStarter) == false)
                return false;

            if (cleanLine.StartsWith(PropertiesLineStarter))
            {
                var match = Regex.Match(cleanLine, PropertyParsingRegex);
                if (match.Success)
                {
                    if (match.Groups[2].Value == "BU_")
                        builder.AddNodeCustomProperty(match.Groups[1].Value, match.Groups[3].Value, match.Groups[9].Value.Replace("\"", ""));
                    else if (match.Groups[2].Value == "EV_")
                        builder.AddEnvironmentVariableCustomProperty(match.Groups[1].Value, match.Groups[3].Value, match.Groups[9].Value.Replace("\"", ""));
                    else if (match.Groups[4].Value == "BO_")
                    {
                        builder.AddMessageCustomProperty(match.Groups[1].Value, uint.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture), match.Groups[9].Value.Replace("\"", ""));
                        if (match.Groups[1].Value == "GenMsgCycleTime")
                            builder.AddMessageCycleTime(uint.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture), int.Parse(match.Groups[9].Value, CultureInfo.InvariantCulture));
                    }
                    else if (match.Groups[6].Value == "SG_")
                    {
                        builder.AddSignalCustomProperty(match.Groups[1].Value, uint.Parse(match.Groups[7].Value, CultureInfo.InvariantCulture), match.Groups[8].Value, match.Groups[9].Value.Replace("\"", ""));
                        if (match.Groups[1].Value == "GenSigStartValue")
                            builder.AddSignalInitialValue(uint.Parse(match.Groups[7].Value, CultureInfo.InvariantCulture), match.Groups[8].Value, double.Parse(match.Groups[9].Value, CultureInfo.InvariantCulture));
                    }
                }
                return true;
            }
            return false;
        }
    }
}