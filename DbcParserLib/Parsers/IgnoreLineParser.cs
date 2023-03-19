namespace DbcParserLib.Parsers
{
    internal class IgnoreLineParser : ILineParser
    {
        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            return line.TrimStart().StartsWith("BS_ ") ||
                    line.TrimStart().StartsWith("NS_ ") ||
                    line.TrimStart().StartsWith("VERSION");
        }
    }
}