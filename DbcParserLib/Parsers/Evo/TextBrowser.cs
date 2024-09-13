using System;
using System.Collections.Generic;

namespace DbcParserLib.Parsers.Evo
{
    public class TextBrowser
    {
        private readonly INextLineProvider m_provider;
        private readonly Queue<ITextFragment> m_items = new Queue<ITextFragment>();

        public TextBrowser(INextLineProvider provider)
        {
            m_provider = provider;
        }

        public int Row { get; private set; }
        public int Col => 0; // TODO: fix this

        public void Walk(Func<char, PeekResult> walker)
        {
            if (m_items.Count == 0)
            {
                if (m_provider.TryGetLine(out var line))
                {
                    m_items.Enqueue(new TextFragment(line));
                }
            }

            while (m_items.Count > 0)
            {
                var currentFragment = m_items.Peek();
                Row += currentFragment.IsNew ? 1 : 0;

                while (currentFragment.TryPeek(out var item))
                {
                    var nextOp = walker(item);

                    if (nextOp == PeekResult.Consume || nextOp == PeekResult.Continue)
                    {
                        currentFragment.Pop();
                    }

                    if (nextOp != PeekResult.Continue)
                        return;

                }

                if (currentFragment.RequiresNewLine && m_provider.TryGetLine(out var line))
                {
                    m_items.Enqueue(new VirtualNewLine(currentFragment.Col));
                    m_items.Enqueue(new TextFragment(line));
                }

                m_items.Dequeue();
            }
        }
    }
}