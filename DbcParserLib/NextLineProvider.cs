using DbcParserLib.Observers;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace DbcParserLib
{
    public class NextLineProvider : INextLineProvider
    {
        private TextReader m_reader;
        private IParseFailureObserver m_observer;
        private string m_lineMemory;
        private bool m_isVirtualLine;

        private string lineTermination = ";";

        private readonly string[] keywords = new[]
        {
            "VERSION",
            "FILTER",

            "NS_DESC_",
            "NS_",

            "CM_",

            "BA_DEF_DEF_REL_",
            "BA_DEF_REL_",
            "BA_REL_",
            "BA_DEF_SGTYPE_",
            "BA_SGTYPE_",
            "BA_DEF_DEF_",
            "BA_DEF_",
            "BA_",

            "CAT_DEF_",
            "CAT_",

            "SGTYPE_VAL_",
            "SGTYPE_",

            "SIGTYPE_VALTYPE_",

            "VAL_TABLE_",
            "VAL_",

            "SIG_GROUP_",
            "SIG_VALTYPE_",
            "SIG_TYPE_REF_",

            "EV_DATA_",
            "ENVVAR_DATA_",

            "BO_TX_BU_",
            "BO_",

            "BU_SG_REL_",
            "BU_EV_REL_",
            "BU_BO_REL_",
            "BU_",

            "SG_MUL_VAL_",
            "SG_",

            "BS_",
        };

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
                if (m_isVirtualLine == false)
                {
                    m_observer.CurrentLine++;
                }
                line = m_lineMemory;    
                line = line.Trim();
                m_lineMemory = null;
                line = HandleMultipleDefinitionsPerLine(line);
                line = HandleMultiline(line);

                return true;
            }

            if (m_reader.Peek() >= 0)
            {
                m_observer.CurrentLine++;
                line = m_reader.ReadLine();
                line = line.Trim();
                line = HandleMultipleDefinitionsPerLine(line);
                line = HandleMultiline(line);

                return true;
            }
            return false;
        }

        private string HandleMultipleDefinitionsPerLine(string line)
        {
            int definitionTerminationLocation = line.IndexOf(lineTermination, StringComparison.Ordinal);

            if (definitionTerminationLocation >= 0)
            {
                if (definitionTerminationLocation + 1 == line.Length)
                {
                    return line;
                }
                var firstLinePart = line.Substring(0, definitionTerminationLocation + 1);
                m_lineMemory = line.Substring(definitionTerminationLocation + 2, line.Length - 1).Trim();
                m_isVirtualLine = true;

                return firstLinePart;
            }
            return line;
        }

        private string HandleMultiline(string line)
        {
            if (line.EndsWith(lineTermination))
            {
                return line;
            }
            var nextLine = PeakNextLine();
            if (string.IsNullOrWhiteSpace(nextLine))
            {
                return line;
            }
            if (CheckNextLineParsing(PeakNextLine()))
            {
                return line;
            }

            TryGetLine(out var actualNextLine);
            return CombineLines(line, actualNextLine);
        }

        private string PeakNextLine()
        {
            if (m_lineMemory == null)
            {
                var nextLine = m_reader.ReadLine();
                if (nextLine is null)
                {
                    return nextLine;
                }
                m_lineMemory = nextLine;
                m_isVirtualLine = false;
                return m_lineMemory;
            }
            else
            {
                return m_lineMemory;
            }
        }

        private bool CheckNextLineParsing(string nextLine)
        {
            nextLine = nextLine.TrimStart();
            return keywords.Any(prefix => nextLine.StartsWith(prefix));
        }

        private string CombineLines(string currentLine, string nextLine)
        {
            currentLine = currentLine.TrimEnd('\r', '\n');

            string combinedLine = currentLine + " " + nextLine;

            return combinedLine;

            /*var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(currentLine);
            stringBuilder.Append(nextLine);
            return stringBuilder.ToString();*/
        }
    }
}