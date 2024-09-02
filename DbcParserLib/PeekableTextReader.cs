using DbcParserLib.Observers;
using System.Collections.Generic;
using System.IO;

namespace DbcParserLib
{
    internal class PeekableTextReader
    {
        private IParseFailureObserver m_observer;
        private readonly TextReader m_underlying;
        private readonly Queue<string> m_bufferedLines;
        private string m_virtualLineMemory;

        public PeekableTextReader(TextReader underlying, IParseFailureObserver observer)
        {
            m_underlying = underlying;
            m_bufferedLines = new Queue<string>();
            m_observer = observer;
        }

        public string PeekLine()
        {
            string line = m_underlying.ReadLine();
            if (line == null)
                return null;
            m_bufferedLines.Enqueue(line);
            return line;
        }
        public void AddVirtualLine(string line)
        {
            m_virtualLineMemory = line;
        }

        public string ReadLine()
        {
            if (m_virtualLineMemory != null)
            {
                var temp = m_virtualLineMemory;
                m_virtualLineMemory = null;
                return temp;
            }
            m_observer.CurrentLine++;
            if (m_bufferedLines.Count > 0)
                return m_bufferedLines.Dequeue();
            return m_underlying.ReadLine();
        }
    }
}
