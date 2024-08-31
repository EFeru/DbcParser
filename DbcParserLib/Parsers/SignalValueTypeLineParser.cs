using System.Text.RegularExpressions;
using DbcParserLib.Model;
using DbcParserLib.Observers;

namespace DbcParserLib.Parsers
{
    internal class SignalValueTypeLineParser : ILineParser
    {
        private const string SignalValueTypeStarter = "SIG_VALTYPE_ ";
        private const string SignalValueTypeParsingRegex = @"SIG_VALTYPE_\s+(\d+)\s+([a-zA-Z_][\w]*)\s+[:]*\s*([0123])\s*;";

        private readonly IParseFailureObserver m_observer;

        public SignalValueTypeLineParser(IParseFailureObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var cleanLine = line.Trim().ReplaceNewlinesWithSpace();

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
            }
            else
                m_observer.SignalValueTypeSyntaxError();
            
            return true;
        }
    }
}