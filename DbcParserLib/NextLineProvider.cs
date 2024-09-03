using DbcParserLib.Observers;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace DbcParserLib
{
    public class NextLineProvider : INextLineProvider
    {
        private PeekableTextReader m_reader;

        private const string lineTermination = ";";

        private readonly string[] keywords =
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
            "EV_",

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
            m_reader = new PeekableTextReader(reader, observer);
        }

        public bool TryGetLine(out string line)
        {
            line = null;

            var readLine = m_reader.ReadLine();

            if (readLine is null)
            {
                return false;
            }

            line = readLine.Trim();

            if (string.IsNullOrEmpty(line))
            {
                return true;
            }

            line = HandleMultipleDefinitionsPerLine(line);
            line = HandleMultiline(line);

            var test = line;
            if (line.EndsWith(lineTermination) == false && keywords.Any(prefix => test.Equals(prefix)) == false) //correct missing terminations
            {
                line = line + lineTermination;
            }

            return true;
        }

        private string HandleMultipleDefinitionsPerLine(string line)
        {
            int definitionTerminationLocation = line.IndexOf(lineTermination, StringComparison.Ordinal);

            var lastTerminationLocation = -1;
            while (definitionTerminationLocation > lastTerminationLocation)
            {
                if (definitionTerminationLocation + 1 == line.Length)
                {
                    return line;
                }

                var partAfterTermination = line.Substring(definitionTerminationLocation + 2, line.Length - 2 - definitionTerminationLocation);

                if (CheckLineIsDefinition(partAfterTermination.TrimStart()))
                {
                    m_reader.SetVirtualLine(partAfterTermination);
                    return line.Substring(0, definitionTerminationLocation + 1);
                }

                lastTerminationLocation = definitionTerminationLocation;
                definitionTerminationLocation = definitionTerminationLocation + 1 + partAfterTermination.IndexOf(lineTermination, StringComparison.Ordinal);
            }
            return line;
        }

        private string HandleMultiline(string line)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(line);

            var numEmptyLines = 0;
            while (true)
            {
                var checkLine = m_reader.PeekLine();

                if (checkLine is null)
                {
                    break;
                }

                checkLine = checkLine.Trim();

                if (string.IsNullOrEmpty(checkLine))
                {
                    numEmptyLines++;
                    continue;
                }

                if (CheckLineIsDefinition(checkLine) == false)
                {
                    for (int i = 0; i < numEmptyLines; i++)
                    {
                        stringBuilder.AppendLine(m_reader.ReadLine().Trim());
                    }
                    numEmptyLines = 0;

                    var lineToAdd = m_reader.ReadLine().Trim();
                    lineToAdd = HandleMultipleDefinitionsPerLine(lineToAdd);

                    stringBuilder.AppendLine(lineToAdd);
                    continue;
                }

                break;
            }

            return stringBuilder.ToString();
        }

        private bool CheckLineIsDefinition(string line)
        {
            return keywords.Any(keywordPrefix => line.StartsWith(keywordPrefix));
        }
    }
}
