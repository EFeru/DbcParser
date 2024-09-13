using System;

namespace DbcParserLib.Parsers.Evo
{
    public class CompactChainBuilder : IChainBuilder
    {
        private readonly TextBrowser m_browser;

        public CompactChainBuilder(TextBrowser browser)
        {
            m_browser = browser;
        }

        public IChainBuilder Vacuum(bool optional = false)
        {
            return this;
        }

        public IChainBuilder One(char value)
        {
            return OneOf(c => c == value);
        }

        public IChainBuilder Read(Predicate<char> take, out string text)
        {
            text = null;
            return (m_browser.TryNext() && m_browser.TryRead(take, out text)) ? this : BrokenChain.Instance;
        }

        public IChainBuilder ReadId(out string text)
        {
            text = null;
            return m_browser.TryNext() && m_browser.TryReadId(out text) ? this : BrokenChain.Instance;
        }

        public IChainBuilder OneOf(Predicate<char> filter)
        {
            var result = false;

            if (m_browser.TryNext())
            {
                m_browser.Walk(item =>
                {
                    if (filter(item))
                    {
                        result = true;
                        return PeekResult.Consume;
                    }

                    return PeekResult.Stop;
                });
            }

            return result ? this : BrokenChain.Instance;
        }

        public bool Assert()
        {
            return true;
        }
    }
}