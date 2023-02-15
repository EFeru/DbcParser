using System.IO;

namespace DbcParserLib
{
    public class NextLineProvider : INextLineProvider
    {
        private TextReader m_reader;

        public NextLineProvider(TextReader reader)
        {
            m_reader = reader;
        }

        public bool TryGetLine(out string line)
        {
            line = null;
            if (m_reader.Peek() >= 0)
            {
                line = m_reader.ReadLine();
                return true;
            }
            return false;
        }
    }
}