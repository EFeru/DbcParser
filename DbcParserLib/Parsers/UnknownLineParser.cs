namespace DbcParserLib.Parsers
{
    internal class UnknownLineParser : ILineParser
    {
        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            // Throw or log or add a specific entry in builder maybe?
            return true;
        }
    }
}