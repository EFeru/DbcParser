namespace DbcParserLib.Parsers
{
    internal class UnknownLineParser : ILineParser
    {
        private readonly IParseObserver m_observer;

        public UnknownLineParser(IParseObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, int lineNumber, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            // Throw or log or add a specific entry in builder maybe?
            return true;
        }
    }
}