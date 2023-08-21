namespace DbcParserLib.Parsers
{
    internal interface ILineParser
    {
        bool TryParse(string line, int lineNumber, IDbcBuilder builder, INextLineProvider nextLineProvider);
    }
}