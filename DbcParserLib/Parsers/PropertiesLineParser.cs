using System.Globalization;
using System.Text.RegularExpressions;
using DbcParserLib.Observers;

namespace DbcParserLib.Parsers
{
    internal class PropertiesLineParser : ILineParser
    {
        private const string PropertiesLineStarter = "BA_ ";
        private const string PropertyParsingRegex = @"BA_\s+""([a-zA-Z_][\w]*)""(?:\s+(?:(BU_|EV_)\s+([a-zA-Z_][\w]*))|\s+(?:(BO_)\s+(\d+))|\s+(?:(SG_)\s+(\d+)\s+([a-zA-Z_][\w]*)))?\s+(-?\d+|[\d\+\-eE.]+|""[^""]*"");";

        private readonly IParseFailureObserver m_observer;

        public PropertiesLineParser(IParseFailureObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var cleanLine = line.Trim(' ');

            if (cleanLine.StartsWith(PropertiesLineStarter) == false)
                return false;

            var match = Regex.Match(cleanLine, PropertyParsingRegex);
            if (match.Success)
            {
                var isNumeric = !match.Groups[9].Value.StartsWith("\"");
                var stringValue = match.Groups[9].Value.Replace("\"", "");

                if (match.Groups[2].Value == "BU_")
                    builder.AddNodeCustomProperty(match.Groups[1].Value, match.Groups[3].Value, stringValue, isNumeric);
                else if (match.Groups[2].Value == "EV_")
                    builder.AddEnvironmentVariableCustomProperty(match.Groups[1].Value, match.Groups[3].Value, stringValue, isNumeric);
                else if (match.Groups[4].Value == "BO_")
                    builder.AddMessageCustomProperty(match.Groups[1].Value, uint.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture), stringValue, isNumeric);
                else if (match.Groups[6].Value == "SG_")
                    builder.AddSignalCustomProperty(match.Groups[1].Value, uint.Parse(match.Groups[7].Value, CultureInfo.InvariantCulture), match.Groups[8].Value, stringValue, isNumeric);
                else
                {
                    builder.AddGlobalCustomProperty(match.Groups[1].Value, match.Groups[9].Value, isNumeric);
                }
            }
            else
                m_observer.PropertySyntaxError();

            return true;
        }
    }
}