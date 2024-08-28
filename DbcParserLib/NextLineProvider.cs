using DbcParserLib.Observers;
using System;
using System.IO;

namespace DbcParserLib
{
    public class NextLineProvider : INextLineProvider
    {
        private TextReader m_reader;
        private IParseFailureObserver m_observer;
        private string m_lineMemory;

        public NextLineProvider(TextReader reader, IParseFailureObserver observer)
        {
            m_reader = reader;
            m_observer = observer;
        }

        public bool TryGetLine(out string line)
        {
            line = null;
            if (m_lineMemory != null)
            {
                line = m_lineMemory;
                line = line.Trim();
                line = HandleMultipleDefinitionsPerLine(line);

                return true;
            }

            if (m_reader.Peek() >= 0)
            {
                m_observer.CurrentLine++;
                line = m_reader.ReadLine();
                line = line.Trim();
                line = HandleMultipleDefinitionsPerLine(line);

                return true;
            }
            return false;
        }

        private string HandleMultipleDefinitionsPerLine(string line)
        {
            int definitionTerminationLocation = line.IndexOf(";", StringComparison.Ordinal);

            if (definitionTerminationLocation >= 0)
            {
                if (definitionTerminationLocation + 1 == line.Length)
                {
                    return line;
                }
                var firstLinePart = line.Substring(0, definitionTerminationLocation + 1);
                m_lineMemory = line.Substring(definitionTerminationLocation + 2, line.Length - 1).Trim();

                return firstLinePart;
            }
            return line;
        }
    }
}