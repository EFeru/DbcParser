using System.Globalization;
using System.Text.RegularExpressions;
using DbcParserLib.Model;
using DbcParserLib.Observers;

namespace DbcParserLib.Parsers
{
    internal class MessageLineParser : ILineParser
    {
        private const string IdGroup = "Id";
        private const string NameGroup = "Name";
        private const string SizeGroup = "Size";
        private const string TransmitterGroup = "Transmitter";
        private const string MessageLineStarter = "BO_ ";

        private readonly string m_messageRegex = $@"BO_ (?<{IdGroup}>\d+)\s+(?<{NameGroup}>[a-zA-Z_][\w]*)\s*:\s*(?<{SizeGroup}>\d+)\s+(?<{TransmitterGroup}>[a-zA-Z_][\w]*)";

        private readonly IParseFailureObserver m_observer;

        public MessageLineParser(IParseFailureObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            if (line.Trim().StartsWith(MessageLineStarter) == false)
                return false;

            var match = Regex.Match(line, m_messageRegex);
            if (match.Success)
            {
                var msg = new Message()
                {
                    Name = match.Groups[NameGroup].Value,
                    DLC = ushort.Parse(match.Groups[SizeGroup].Value, CultureInfo.InvariantCulture),
                    Transmitter = match.Groups[TransmitterGroup].Value,
                    ID = uint.Parse(match.Groups[IdGroup].Value, CultureInfo.InvariantCulture)
                };

                builder.AddMessage(msg);
            }
            else
                m_observer.MessageSyntaxError();

            return true;
        }
    }
}