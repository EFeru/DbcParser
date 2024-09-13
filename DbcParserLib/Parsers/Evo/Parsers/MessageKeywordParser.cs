namespace DbcParserLib.Parsers.Evo.Parsers
{
    public class MessageKeywordParser : IKeywordParser
    {
        public IKeywordParser TryParse(TextBrowser browser, IKeywordStore store)
        {
            var result = browser.Chain()
                .Vacuum(true)
                .Read(char.IsDigit, out var messageId)
                .Vacuum()
                .ReadId(out var messageName)
                .Vacuum(true)
                .One(CharExtensions.Colon)
                .Vacuum(true)
                .Read(char.IsDigit, out var messageLength)
                .Assert();


            if (result)
            {
                while (browser.TryNext() && browser.TryReadId(out var text))
                {
                    if (store.TryGetKeywordParser(text, out var parser))
                    {
                        return parser;
                    }

                    // Add node named text as message tx;
                    // Create message
                }
            }
            return null;
        }
    }
}