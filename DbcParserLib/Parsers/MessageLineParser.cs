using System.Globalization;
using System.Text.RegularExpressions;
using DbcParserLib.Model;
using DbcParserLib.Observers;

namespace DbcParserLib.Parsers
{
    internal class MessageLineParser : ILineParser
    {
        private const string MessageLineStarter = "BO_ ";
        private const string MessageRegex = @"BO_ (\d+)\s+([a-zA-Z_][\w]*)\s*:\s*(\d+)\s+([a-zA-Z_][\w]*)";

        private readonly IParseFailureObserver m_observer;

        public MessageLineParser(IParseFailureObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var cleanLine = line.ReplaceNewlinesWithSpace().Trim();

            if (cleanLine.StartsWith(MessageLineStarter) == false)
                return false;
            
            var match = Regex.Match(cleanLine, MessageRegex);
            if(match.Success)
            {
                var msg = new Message()
                {
                    Name = match.Groups[2].Value,
                    DLC = ushort.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture),
                    Transmitter = match.Groups[4].Value,
                    ID = uint.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)
                };
                
                builder.AddMessage(msg);
            }
            else
                m_observer.MessageSyntaxError();
            
            return true;
        }
    }
}