namespace DbcParserLib.Parsers
{
    public interface ILineParser
    {
        bool TryParse(string line, DbcBuilder builder);
    }
}