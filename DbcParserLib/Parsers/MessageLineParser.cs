using System.Globalization;

namespace DbcParserLib.Parsers
{
    public class MessageLineParser : ILineParser
    {
        private const string MessageLineStarter = "BO_";

        public bool TryParse(string line, DbcBuilder builder)
        {
            if(line.TrimStart().StartsWith(MessageLineStarter) == false)
                return false;
            
            string[] record = line.Split(' ');

            var msg = new Message()
            {
                Name = record[2].Substring(0, record[2].Length - 1),
                DLC = byte.Parse(record[3], CultureInfo.InvariantCulture),
                Transmitter = record[4],
            };
            msg.ID = uint.Parse(record[1], CultureInfo.InvariantCulture);
            msg.IsExtID = CheckExtID(ref msg.ID);

            builder.AddMessage(msg);

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