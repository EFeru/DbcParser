namespace DbcParserLib.Parsers
{
    public class UnknownLineParser : ILineParser
    {
        public bool TryParse(string line, DbcBuilder builder)
        {
            // Throw or log or add a specific entry in builder maybe?
            return true;
        }
    }
}