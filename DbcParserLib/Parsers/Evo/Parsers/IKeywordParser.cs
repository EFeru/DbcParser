namespace DbcParserLib.Parsers.Evo.Parsers
{
    public interface IKeywordParser
    {
        IKeywordParser TryParse(TextBrowser browser, IKeywordStore store);
    }
}