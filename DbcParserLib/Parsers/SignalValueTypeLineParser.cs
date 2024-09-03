using System.Text.RegularExpressions;
using DbcParserLib.Model;
using DbcParserLib.Observers;

namespace DbcParserLib.Parsers
{
    internal class SignalValueTypeLineParser : ILineParser
    {
        private const string MessageIdGroup = "MessageId";
        private const string SignalNameGroup = "SignalName";
        private const string SignalTypeGroup = "SignalType";
        private const string SignalValueTypeStarter = "SIG_VALTYPE_ ";

        private readonly string m_signalValueTypeParsingRegex = $@"SIG_VALTYPE_\s+(?<{MessageIdGroup}>\d+)\s+(?<{SignalNameGroup}>[a-zA-Z_][\w]*)\s+[:]*\s*(?<{SignalTypeGroup}>[0123])\s*;";

        private readonly IParseFailureObserver m_observer;

        public SignalValueTypeLineParser(IParseFailureObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var cleanLine = line.ReplaceNewlinesWithSpace().Trim();

            if (cleanLine.StartsWith(SignalValueTypeStarter) == false)
                return false;

            var match = Regex.Match(cleanLine, m_signalValueTypeParsingRegex);
            if (match.Success)
            {
                var valueType = uint.Parse(match.Groups[SignalTypeGroup].Value);
                if (valueType == 1 || valueType == 2)
                {
                    builder.AddSignalValueType(uint.Parse(match.Groups[MessageIdGroup].Value), match.Groups[SignalNameGroup].Value,
                        valueType == 1 ? DbcValueType.IEEEFloat : DbcValueType.IEEEDouble);
                }
            }
            else
                m_observer.SignalValueTypeSyntaxError();

            return true;
        }
    }
}