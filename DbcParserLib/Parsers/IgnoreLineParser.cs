using DbcParserLib.Observers;

namespace DbcParserLib.Parsers
{
    internal class IgnoreLineParser : ILineParser
    {
        private readonly IParseFailureObserver m_observer;

        public IgnoreLineParser(IParseFailureObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            return line.TrimStart().StartsWith("VERSION") ||
                   line.TrimStart().StartsWith("BS_ ") ||
                   line.TrimStart().StartsWith("BS_") ||
                   line.TrimStart().StartsWith("NS_ ") ||
                   line.Trim().Equals("CM_") ||
                   line.Trim().Equals("BA_DEF_DEF_") ||
                   line.Trim().Equals("BA_DEF_") ||
                   line.Trim().Equals("BA_") ||
                   line.Trim().Equals("VAL_") ||
                   line.Trim().Equals("EV_DATA_") ||
                   line.Trim().Equals("ENVVAR_DATA_") ||
                   line.Trim().Equals("SIG_VALTYPE_") ||
                   line.Trim().Equals("VAL_TABLE_");
        }
    }
}