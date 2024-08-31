using DbcParserLib.Observers;

namespace DbcParserLib.Parsers
{
    internal class UnknownLineParser : ILineParser
    {
        private readonly IParseFailureObserver m_observer;

        public UnknownLineParser(IParseFailureObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            m_observer.UnknownLine(line);
            return true;
        }
    }
}