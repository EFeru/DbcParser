namespace DbcParserLib.Parsers
{
    public class IgnoreLineParser : ILineParser
    {
        public bool TryParse(string line, DbcBuilder builder)
        {
            return line.TrimStart().StartsWith("BS_") ||
                    line.TrimStart().StartsWith("NS_") ||
                    line.TrimStart().StartsWith("VERSION");
        }
    }
}