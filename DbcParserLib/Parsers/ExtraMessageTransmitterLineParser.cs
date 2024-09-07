using DbcParserLib.Observers;
using System;
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
                return false;

            var match = Regex.Match(line, m_extraTransmitterRegex);
            if (match.Success)
            {
                Console.WriteLine(match.Groups[MessageId]);
                Console.WriteLine(match.Groups[TransmitterGroup].Value);
            }
            else
            {
                m_observer.ExtraMessageTransmittersSyntaxError();
            }

            return true;
        }
    }
}
