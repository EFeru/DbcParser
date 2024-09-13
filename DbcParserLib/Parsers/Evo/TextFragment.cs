namespace DbcParserLib.Parsers.Evo
{
    public class TextFragment : ITextFragment
    {
        private readonly string m_text;
        private int m_cursor = 0;

        public TextFragment(string fragment)
        {
            m_text = fragment;
            IsNew = true;
        }

        public bool TryPeek(out char item)
        {
            IsNew = false;
            if (m_cursor < m_text.Length)
            {
                item = m_text[m_cursor];
                return true;
            }

            item = '\0';

            return false;
        }

        public void Pop()
        {
            ++m_cursor;
        }

        public bool RequiresNewLine => true;
        public bool IsNew { get; private set; }
        public int Col => m_cursor + 1;
    }
}