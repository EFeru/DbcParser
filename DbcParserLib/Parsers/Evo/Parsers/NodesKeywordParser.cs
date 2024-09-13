namespace DbcParserLib.Parsers.Evo.Parsers
{
    public class NodesKeywordParser : IKeywordParser
    {
        public IKeywordParser TryParse(TextBrowser browser, IKeywordStore store)
        {
            var result = browser.Chain()
                .Vacuum(true)
                .One(CharExtensions.Colon)
                .Vacuum(true)
                .Assert();

            if (result)
            {
                while (browser.Chain()
                       .ReadId(out var nodeName)
                       .Vacuum()
                       .Assert())
                {
                    if (store.TryGetKeywordParser(nodeName, out var parser))
                    {
                        return parser;
                    }
                    // Add node named text;
                }
            }

            return null;
        }
    }
}