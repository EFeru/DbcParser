using DbcParserLib.Observers;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DbcParserLib.Parsers 
{
    internal class ExtraMessageTransmitterLineParser : ILineParser
    {
        private const string MessageId = "Id";
        private const string TransmitterGroup = "Transmitter";
        private const string ExtraMessageTransmitterLineStarter = "BO_TX_BU_ ";

        private readonly string m_extraTransmitterRegex = $@"BO_TX_BU_ (?<{MessageId}>\d+)\s*:\s*(?<{TransmitterGroup}>(\s*(?:[a-zA-Z_][\w]*)\s*(?:,)?)+);";

        private readonly IParseFailureObserver m_observer;

        public ExtraMessageTransmitterLineParser(IParseFailureObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            if (line.TrimStart().StartsWith(ExtraMessageTransmitterLineStarter) == false)
            {
                return false;
            }

            var match = Regex.Match(line, m_extraTransmitterRegex);
            if (match.Success)
            {
                var messageId = uint.Parse(match.Groups[MessageId].Value, CultureInfo.InvariantCulture);
                var transmitters = new List<string>();

                foreach (var transmitter in match.Groups[TransmitterGroup].Value.Trim().Split(','))
                {
                    var transmitterClean = transmitter.Trim();
                    if (transmitters.Contains(transmitterClean))
                    {
                        m_observer.ExtraMessageTransmittersDuplicate(messageId, transmitterClean);
                        continue;
                    }
                    transmitters.Add(transmitterClean);
                }

                builder.AddMessageAdditionalTransmitters(messageId, transmitters.ToArray());
            }
            else
            {
                m_observer.ExtraMessageTransmittersSyntaxError();
            }

            return true;
        }
    }
}
