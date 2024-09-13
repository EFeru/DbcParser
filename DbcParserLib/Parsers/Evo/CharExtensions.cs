namespace DbcParserLib.Parsers.Evo
{
    public static class CharExtensions
    {
        internal const char Underscore = '_';
        internal const char Colon = ':';
        internal const char SemiColon = ';';
        internal const char DoubleQuote = '"';
        internal const char Dot = '.';
        internal const char Comma = ',';

        public static bool IsIdentifierStart(this char item)
        {
            return char.IsLetter(item) || item.Is(Underscore);
        }

        public static bool IsUnderscore(this char item)
        {
            return item.Is(Underscore);
        }

        public static bool Any(this char item)
        {
            return true;
        }

        public static bool IsDot(this char item)
        {
            return item.Is(Dot);
        }

        private static bool Is(this char item, int value)
        {
            return item == value;
        }

        public static bool IsDoubleQuote(this char item)
        {
            return item.Is(DoubleQuote);
        }

        public static bool IsColon(this char item)
        {
            return item.Is(Colon);
        }

        public static bool IsSemiColon(this char item)
        {
            return item.Is(SemiColon);
        }

        public static bool IsKeywordEnd(this char item)
        {
            return char.IsWhiteSpace(item) || item.IsColon();
        }
    }
}