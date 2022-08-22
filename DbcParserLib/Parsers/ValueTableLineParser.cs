using System;
using System.Text;

namespace DbcParserLib.Parsers
{
    public class ValueTableLineParser : ILineParser
    {
        private const string ValueTableLineStarter = "VAL_";

        public bool TryParse(string line, DbcBuilder builder)
        {
            if(line.TrimStart().StartsWith(ValueTableLineStarter) == false)
                return false;


            string[] records = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if(line.TrimStart().StartsWith("VAL_TABLE"))
            {
                builder.AddNamedValueTable(records[1], records[2]);
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
                        var withoutQuotes = records[i+1].Trim(';', '"');
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