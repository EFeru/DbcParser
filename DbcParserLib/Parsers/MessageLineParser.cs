using System.Globalization;
using System.Text.RegularExpressions;
using DbcParserLib.Model;

namespace DbcParserLib.Parsers
{
    internal class MessageLineParser : ILineParser
    {
        private const string MessageLineStarter = "BO_ ";
        private const string MessageRegex = @"BO_ (\d+)\s+(\w+)\s*:\s*(\d+)\s+(\w+)";

        private readonly IParseObserver m_observer;

        public MessageLineParser(IParseObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, int lineNumber, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            if(line.Trim().StartsWith(MessageLineStarter) == false)
                return false;
            
            var match = Regex.Match(line, MessageRegex);
            if(match.Success)
            {
                var msg = new Message()
                {
                    Name = match.Groups[2].Value,
                    DLC = byte.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture),
                    Transmitter = match.Groups[4].Value,
                    ID = uint.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)
                };
                
                builder.AddMessage(msg);
            }

            return true;
        }
    }
}