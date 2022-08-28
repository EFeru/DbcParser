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
                .SplitBySpace();

            if (records.Length == 1)
                return false;

            if (line.TrimStart().StartsWith("VAL_TABLE_ "))
            {
                if (records.Length > 2 && (records.Length - 2) % 2 == 0)
                    builder.AddNamedValueTable(records[1], Helpers.ConvertToMultiLine(records, 2));

                return true;
            }
            
            var parsed = false;
            if(line.TrimStart().StartsWith("VAL_ "))
            {
                parsed = true;

                if(uint.TryParse(records[1], out var messageId))
                {
                    if (records.Length == 4)
                    {
                        // Last is a table name
                        builder.LinkNamedTableToSignal(messageId, records[2], records[3]);
                    }
                    else if (records.Length > 4 && (records.Length - 3) % 2 == 0)
                    {
                        builder.LinkTableValuesToSignal(messageId, records[2], Helpers.ConvertToMultiLine(records, 3));
                    }
                }         
            }
            
            return parsed;
        }
    }
}