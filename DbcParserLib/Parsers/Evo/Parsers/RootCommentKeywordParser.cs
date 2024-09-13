namespace DbcParserLib.Parsers.Evo.Parsers
{
    public class RootCommentKeywordParser : IKeywordParser
    {
        public IKeywordParser TryParse(TextBrowser browser, IKeywordStore store)
        {
            var result = browser.Chain()
                .Vacuum(true)
                .One(CharExtensions.DoubleQuote)
                .Read(c => c != CharExtensions.DoubleQuote, out var comment) // We won't accept escaped double quotes
                .One(CharExtensions.DoubleQuote)
                .Vacuum(true)
                .One(CharExtensions.SemiColon)
                .Assert();

            if (result)
            {
                // Set comment to message ID

                return new GenericNextKeywordFinder();
            }

            return null;
        }
    }
}