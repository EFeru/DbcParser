using System;
using System.Text;

namespace DbcParserLib.Parsers.Evo
{
    public static class TextBrowserExtensions
    {
        public static bool TryNext(this TextBrowser browser)
        {
            return
                browser.TrySkip(char.IsWhiteSpace);
        }

        public static bool TryOne(this TextBrowser browser, char value)
        {
            return browser.TryOne(PeekResult.Consume, value);
        }

        public static bool CheckOne(this TextBrowser browser, char value)
        {
            return browser.TryOne(PeekResult.Stop, value);
        }

        private static bool PickOne(this TextBrowser browser, out char value)
        {
            var result = false;
            var outItem = '\0';

            browser.Walk(item =>
            {
                outItem = item;
                result = true;

                return PeekResult.Consume;
            });

            value = outItem;
            return result;
        }

        private static bool TryOne(this TextBrowser browser, PeekResult returnValue, char value)
        {
            var result = false;

            browser.Walk(item =>
            {
                if (value == item)
                {
                    result = true;
                    return returnValue;
                }

                return PeekResult.Stop;
            });

            return result;
        }

        public static bool TrySkip(this TextBrowser browser, Predicate<char> skip)
        {
            return browser.TrySkip(skip, PeekResult.Stop, out _);
        }

        public static bool TrySkip(this TextBrowser browser, Predicate<char> skip, out char character)
        {
            return browser.TrySkip(skip, PeekResult.Consume, out character);
        }

        private static bool TrySkip(this TextBrowser browser, Predicate<char> skip, PeekResult returnValue, out char character)
        {
            var result = false;
            var value = '\0';

            browser.Walk(item =>
            {
                if (skip(item))
                    return PeekResult.Continue;

                value = item;

                result = true;
                return returnValue;
            });

            character = value;
            return result;
        }

        public static bool TryRead(this TextBrowser browser, Predicate<char> take, out string text)
        {
            var sb = new StringBuilder();

            browser.Walk(item =>
            {
                if (take(item))
                {
                    sb.Append(item);
                    return PeekResult.Continue;
                }

                return PeekResult.Stop;
            });

            text = sb.ToString();

            return sb.Length > 0;
        }


        public static bool TryReadId(this TextBrowser browser, out string text)
        {
            var sb = new StringBuilder();

            if (browser.PickOne(out var firstChar) && (char.IsLetter(firstChar) || firstChar.IsUnderscore()))
            {
                sb.Append(firstChar);

                if (browser.TryRead(c => char.IsLetterOrDigit(c) || c.IsUnderscore(), out var id))
                {
                    sb.Append(id);
                }
            }

            text = sb.ToString();
            return sb.Length > 0; // ID has minimum length?
        }

        public static IChainBuilder Chain(this TextBrowser browser)
        {
            return new ChainBuilder(browser);
        }

        public static IChainBuilder CompactChain(this TextBrowser browser)
        {
            return new CompactChainBuilder(browser);
        }
    }
}