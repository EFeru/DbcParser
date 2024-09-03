using System.Globalization;
using System.Text.RegularExpressions;
using DbcParserLib.Observers;

namespace DbcParserLib.Parsers
{
    internal class PropertiesLineParser : ILineParser
    {
        private const string AttributeNameGroup = "AttributeName";
        private const string AttributeValueGroup = "AttributeValue";
        private const string IsNodeEnvGroup = "NodeEnv";
        private const string NodeEnvNameGroup = "NodeName";
        private const string IsMessageGroup = "Message";
        private const string MessageIdGroup = "MessageId";
        private const string IsSignalGroup = "Signal";
        private const string MessageSignalIdGroup = "MessageId";
        private const string SignalNameGroup = "SignalName";
        private const string PropertiesLineStarter = "BA_ ";

        private readonly string m_propertyParsingRegex = $@"BA_\s+""(?<{AttributeNameGroup}>[a-zA-Z_][\w]*)""(?:\s+(?:(?<{IsNodeEnvGroup}>BU_|EV_)\s+(?<{NodeEnvNameGroup}>[a-zA-Z_][\w]*))|\s+" +
                                                         $@"(?:(?<{IsMessageGroup}>BO_)\s+(?<{MessageIdGroup}>\d+))|\s+(?:(?<{IsSignalGroup}>SG_)\s+(?<{MessageSignalIdGroup}>\d+)\s+" +
                                                         $@"(?<{SignalNameGroup}>[a-zA-Z_][\w]*)))?\s+(?<{AttributeValueGroup}>-?\d+|[\d\+\-eE.]+|""[^""]*"");";
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

            var match = Regex.Match(cleanLine, m_propertyParsingRegex);
            if (match.Success)
            {
                var isNumeric = !match.Groups[AttributeValueGroup].Value.StartsWith(Helpers.DoubleQuotes);
                var stringValue = match.Groups[AttributeValueGroup].Value.Replace(Helpers.DoubleQuotes, string.Empty);

                if (match.Groups[IsNodeEnvGroup].Value.Equals(string.Empty) == false)
                    builder.AddNodeCustomProperty(match.Groups[AttributeNameGroup].Value, match.Groups[NodeEnvNameGroup].Value, stringValue, isNumeric);
                else if (match.Groups[IsNodeEnvGroup].Value.Equals(string.Empty) == false)
                    builder.AddEnvironmentVariableCustomProperty(match.Groups[AttributeNameGroup].Value, match.Groups[NodeEnvNameGroup].Value, stringValue, isNumeric);
                else if (match.Groups[IsMessageGroup].Value.Equals(string.Empty) == false)
                    builder.AddMessageCustomProperty(match.Groups[AttributeNameGroup].Value, uint.Parse(match.Groups[MessageIdGroup].Value, CultureInfo.InvariantCulture), stringValue, isNumeric);
                else if (match.Groups[IsSignalGroup].Value.Equals(string.Empty) == false)
                    builder.AddSignalCustomProperty(match.Groups[AttributeNameGroup].Value, uint.Parse(match.Groups[MessageSignalIdGroup].Value, CultureInfo.InvariantCulture), match.Groups[SignalNameGroup].Value, stringValue, isNumeric);
                else
                {
                    builder.AddGlobalCustomProperty(match.Groups[AttributeNameGroup].Value, stringValue, isNumeric);
                }
            }
            else
                m_observer.PropertySyntaxError();

            return true;
        }
    }
}