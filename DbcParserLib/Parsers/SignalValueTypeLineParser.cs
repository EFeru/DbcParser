using System.Linq;
using DbcParserLib.Model;

namespace DbcParserLib.Parsers
{
    internal class SignalValueTypeLineParser : ILineParser
    {
        private const string SignalValueTypeStarter = "SIG_VALTYPE_ "; 

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var cleanLine = line.Trim(' ', ';');

            if (cleanLine.StartsWith(SignalValueTypeStarter) == false)
                return false;

            string[] records = cleanLine.SplitBySpace();
            if (records.Length > 3 && uint.TryParse(records[1], out var messageId) && byte.TryParse(records[3], out var valueType))
            {
                if (valueType == 1 || valueType == 2)
                {
                    builder.AddSignalValueType(messageId, records[2],
                        valueType == 1 ? DbcValueType.IEEEFloat : DbcValueType.IEEEDouble);
                }

                return true;
            }

            return false;
        }
    }
}