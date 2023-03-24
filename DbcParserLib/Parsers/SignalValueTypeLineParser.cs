using System.Linq;
using System.Text.RegularExpressions;
using DbcParserLib.Model;

namespace DbcParserLib.Parsers
{
    internal class SignalValueTypeLineParser : ILineParser
    {
        private const string SignalValueTypeStarter = "SIG_VALTYPE_ ";
        private const string SignalValueTypeParsingRegex = @"SIG_VALTYPE_\s+(\d+)\s+([a-zA-Z_][\w]*)\s+([0123])\s*;";

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var cleanLine = line.Trim(' ');

            if (cleanLine.StartsWith(SignalValueTypeStarter) == false)
                return false;

            var match = Regex.Match(cleanLine, SignalValueTypeParsingRegex);
            if (match.Success)
            {
                var valueType = uint.Parse(match.Groups[3].Value);
                if (valueType == 1 || valueType == 2)
                {
                    builder.AddSignalValueType(uint.Parse(match.Groups[1].Value), match.Groups[2].Value,
                        valueType == 1 ? DbcValueType.IEEEFloat : DbcValueType.IEEEDouble);
                }
                return true;
            }
            return false;
        }
    }
}