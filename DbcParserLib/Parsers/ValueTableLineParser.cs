using System;
using System.Text;
using System.Linq;

namespace DbcParserLib.Parsers
{
    public class ValueTableLineParser : ILineParser
    {
        private const string ValueTableLineStarter = "VAL_";

        public bool TryParse(string line, IDbcBuilder builder)
        {
            if(line.TrimStart().StartsWith(ValueTableLineStarter) == false)
                return false;

            var records = line
                .Trim(' ', ';')
                .Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (records.Length == 1)
                return false;

            if (line.TrimStart().StartsWith("VAL_TABLE_ "))
            {
                builder.AddNamedValueTable(records[1], string.Join(" ", records.Skip(2)));
                return true;
            }
            
            var parsed = false;
            if(uint.TryParse(records[1], out var messageId))
            {
                if(records.Length == 4)
                {
                    // Last is a table name
                    builder.LinkNamedTableToSignal(messageId, records[2], records[3]);
                    parsed = true;
                }
                else if(records.Length > 4)
                {
                    var sb = new StringBuilder();
                    for(var i = 3; i < records.Length - 1; i += 2)
                    {
                        var withoutQuotes = records[i+1];
                        sb.AppendLine($"{records[i]} {withoutQuotes}");
                    }
                    builder.LinkTableValuesToSignal(messageId, records[2], sb.ToString());
                    parsed = true;
                }    
            }
            
            return parsed;
        }
    }
}