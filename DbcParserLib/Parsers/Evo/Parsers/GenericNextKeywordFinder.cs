namespace DbcParserLib.Parsers.Evo.Parsers
{
    public class GenericNextKeywordFinder : IKeywordParser
    {
        public IKeywordParser TryParse(TextBrowser browser, IKeywordStore store)
        {
            if (browser.Chain().Vacuum(true).ReadId(out var keyword).Assert() && store.TryGetKeywordParser(keyword, out var parser))
            {
                return parser;
            }
            return null;
        }
    }
}