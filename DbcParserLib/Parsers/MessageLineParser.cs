using System.Globalization;
using System.Text.RegularExpressions;

namespace DbcParserLib.Parsers
{
    public class MessageLineParser : ILineParser
    {
        private const string MessageLineStarter = "BO_ ";
        private const string MessageRegex = @"BO_ (\d+)\s+(\w+)\s*:\s*(\d+)\s+(\w+)";

        public bool TryParse(string line, IDbcBuilder builder)
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
                };
                msg.ID = uint.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                msg.IsExtID = CheckExtID(ref msg.ID);

                builder.AddMessage(msg);
            }

            return true;
        }

        private bool CheckExtID(ref uint id)
        {
            // For extended ID bit 31 is always 1
            if (id >= 0x80000000)
            {
                id -= 0x80000000;
                return true;
            }
            else
                return false;
        }
    }
}