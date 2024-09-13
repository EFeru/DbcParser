using System;

namespace DbcParserLib.Parsers.Evo
{
    public class ChainBuilder : IChainBuilder
    {
        private readonly TextBrowser m_browser;

        public ChainBuilder(TextBrowser browser)
        {
            m_browser = browser;
        }

        public IChainBuilder Vacuum(bool optional = false)
        {
            var result = optional;

            m_browser.Walk(item =>
            {
                if (char.IsWhiteSpace(item))
                {
                    result = true;
                    return PeekResult.Continue;
                }

                return PeekResult.Stop;
            });

            return result ? this : BrokenChain.Instance;
        }

        public IChainBuilder One(char value)
        {
            return OneOf(c => c == value);
        }

        public IChainBuilder Read(Predicate<char> take, out string text)
        {
            return m_browser.TryRead(take, out text) ? this : BrokenChain.Instance;
        }

        public IChainBuilder ReadId(out string text)
        {
            return m_browser.TryReadId(out text) ? this : BrokenChain.Instance;
        }

        public IChainBuilder OneOf(Predicate<char> filter)
        {
            var result = false;

            m_browser.Walk(item =>
            {
                if (filter(item))
                {
                    result = true;
                    return PeekResult.Consume;
                }

                return PeekResult.Stop;
            });

            return result ? this : BrokenChain.Instance;
        }

        public bool Assert()
        {
            return true;
        }
    }
}