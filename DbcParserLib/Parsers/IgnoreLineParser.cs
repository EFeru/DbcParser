namespace DbcParserLib.Parsers
{
    internal class IgnoreLineParser : ILineParser
    {
        private readonly IParseObserver m_observer;

        public IgnoreLineParser(IParseObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, int lineNumber, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            return line.TrimStart().StartsWith("BS_ ") ||
                    line.TrimStart().StartsWith("NS_ ") ||
                    line.TrimStart().StartsWith("VERSION");
        }
    }
}