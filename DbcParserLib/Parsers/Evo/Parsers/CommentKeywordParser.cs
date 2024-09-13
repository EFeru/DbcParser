using System.Collections.Generic;

namespace DbcParserLib.Parsers.Evo.Parsers
{
    public class CommentKeywordParser : IKeywordParser
    {
        private static readonly IDictionary<string, IKeywordParser> CommentInnerParsers;

        static CommentKeywordParser()
        {
            CommentInnerParsers = new Dictionary<string, IKeywordParser>()
            {
                {"BO_", new MessageCommentKeywordParser()}
            };

        }
        public IKeywordParser TryParse(TextBrowser browser, IKeywordStore store)
        {
            if (browser.TryNext() && browser.CheckOne(CharExtensions.DoubleQuote))
                return new RootCommentKeywordParser();

            var result = browser.Chain()
                .ReadId(out var secondItem)
                .Assert();

            if (result && CommentInnerParsers.TryGetValue(secondItem, out var parser))
            {
                return parser;
            }

            return null;
        }
    }
}