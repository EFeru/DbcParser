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
                   line.TrimStart().StartsWith("BS_") ||
                   line.TrimStart().StartsWith("NS_ ") ||
                   line.TrimStart().StartsWith("NS_DESC_") ||
                   line.Trim().Equals("CM_") ||
                   line.Trim().Equals("BA_DEF_") ||
                   line.Trim().Equals("BA_") ||
                   line.Trim().Equals("VAL_") ||
                   line.Trim().Equals("CAT_DEF_") ||
                   line.Trim().Equals("CAT_") ||
                   line.Trim().Equals("FILTER") ||
                   line.Trim().Equals("BA_DEF_DEF_") ||
                   line.Trim().Equals("EV_DATA_") ||
                   line.Trim().Equals("ENVVAR_DATA_") ||
                   line.Trim().Equals("SGTYPE_") ||
                   line.Trim().Equals("SGTYPE_VAL_") ||
                   line.Trim().Equals("BA_DEF_SGTYPE_") ||
                   line.Trim().Equals("BA_SGTYPE_") ||
                   line.Trim().Equals("SIG_TYPE_REF_") ||
                   line.Trim().Equals("VAL_TABLE_") ||
                   line.Trim().Equals("SIG_GROUP_") ||
                   line.Trim().Equals("SIG_VALTYPE_") ||
                   line.Trim().Equals("SIGTYPE_VALTYPE_") ||
                   line.Trim().Equals("BO_TX_BU_") ||
                   line.Trim().Equals("BA_DEF_REL_") ||
                   line.Trim().Equals("BA_REL_") ||
                   line.Trim().Equals("BA_DEF_DEF_REL_") ||
                   line.Trim().Equals("BU_SG_REL_") ||
                   line.Trim().Equals("BU_EV_REL_") ||
                   line.Trim().Equals("BU_BO_REL_") ||
                   line.Trim().Equals("SG_MUL_VAL_");
        }
    }
}