namespace DbcParserLib.Parsers.Evo
{
    public class VirtualNewLine : ITextFragment
    {
        private bool m_popped = false;

        public VirtualNewLine(int col)
        {
            Col = col;
        }

        public bool TryPeek(out char item)
        {
            item = m_popped ? '\0' : '\n';
            return m_popped == false;
        }

        public void Pop()
        {
            m_popped = true;
        }

        public bool RequiresNewLine => false;
        public bool IsNew => false;
        public int Col { get;  }
    }
}