using DbcParserLib.Observers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DbcParserLib
{
    public class NextLineProvider : INextLineProvider
    {
        private PeekableTextReader m_reader;
        private string m_virtualLineMemory;

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

            "EV_",
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
            m_reader = new PeekableTextReader(reader, observer);
        }

        public bool TryGetLine(out string line)
        {
            line = null;
            if (m_virtualLineMemory != null)
            {
                line = m_virtualLineMemory.Trim();    
                m_virtualLineMemory = null;

                if (string.IsNullOrEmpty(line))
                {
                    Console.WriteLine($"Line: '{line}'"); //ToDo: remove
                    return true;
                }

                line = HandleMultipleDefinitionsPerLine(line);
                line = HandleMultiline(line);

                Console.WriteLine($"Line: '{line}'"); //ToDo: remove
                return true;
            }

            var readLine = m_reader.ReadLine();
            if (readLine != null)
            {
                line = readLine.Trim();

                if (string.IsNullOrEmpty(line))
                {
                    Console.WriteLine($"Line: '{line}'"); //ToDo: remove
                    return true;
                }

                line = HandleMultipleDefinitionsPerLine(line);
                line = HandleMultiline(line);

                Console.WriteLine($"Line: '{line}'"); //ToDo: remove
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

                var partAfterTermination = line.Substring(definitionTerminationLocation + 2, line.Length - 2 - definitionTerminationLocation).Trim();
                
                if (CheckNextLineParsing(partAfterTermination)) // check if the remaining line is a new definition. otherwise assume your reading a comment
                {
                    m_virtualLineMemory = partAfterTermination;
                    return line.Substring(0, definitionTerminationLocation + 1);
                }

                // Assuming the line is a comment now => dont check for further occurences of the termination for now as they most likely will also just be comment or end of line
                // Would be a very special case were a comment is followed by a definition in the very same line. Could be handled but not for now
            }
            return line;
        }

        private string HandleMultiline(string line)
        {
            var stringsList = new List<string> { line };

            var numEmptyLines = 0;
            while (true)
            {
                var checkLine = m_reader.PeekLine();

                if (checkLine is null)
                {
                    break;
                }

                if (string.IsNullOrEmpty(checkLine.Trim()))
                {
                    numEmptyLines++;
                    continue;
                }

                if (CheckNextLineParsing(checkLine.Trim()) == false)
                {
                    for (int i = 0; i < numEmptyLines; i++)
                    {
                        stringsList.Add(m_reader.ReadLine().Trim());
                    }
                    numEmptyLines = 0;
                    stringsList.Add(m_reader.ReadLine().Trim());
                    continue;
                }

                break;
            }

            var stringBuilder = new StringBuilder();
            for (int i = 0; i < stringsList.Count - 1; i++)
            {
                stringBuilder.AppendLine(stringsList[i]);
            }
            stringBuilder.Append(stringsList.Last());

            return stringBuilder.ToString();
        }

        private bool CheckNextLineParsing(string nextLine)
        {
            nextLine = nextLine.TrimStart();
            return keywords.Any(prefix => nextLine.StartsWith(prefix));
        }
    }
}
