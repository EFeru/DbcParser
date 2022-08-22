namespace DbcParserLib.Parsers
{
    public interface ILineParser
    {
        bool TryParse(string line, IDbcBuilder builder);
    }
}