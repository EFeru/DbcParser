using DbcParserLib.Observers;
using System.Text.RegularExpressions;

namespace DbcParserLib.Parsers
{
    internal class ExtendedMultiplexingLineParser : ILineParser
    {
        private const string SignalLineStarter = "SG_MUL_VAL_ ";
        private const string SignalRegex = @"SG_MUL_VAL_\s+(\d+)\s+(\S+)\s+(\S+)\s+((?:\d+-\d+,?\s*)+);?";

        private readonly IParseFailureObserver m_observer;

        public ExtendedMultiplexingLineParser(IParseFailureObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            if (!line.TrimStart().StartsWith(SignalLineStarter))
            {
                return false;
            }

            var match = Regex.Match(line, SignalRegex);

            if (match.Success)
            {
                var multiplexorSignal = match.Groups[3].Value;
                var multiplexorRanges = match.Groups[4].Value;
                /*var rangesArray = multiplexorRanges.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var range in rangesArray)
                {
                    var rangeClean = range.Trim();
                    var numbers = rangeClean.Split('-');

                    var upper = uint.Parse(numbers[0]);
                    var lower = uint.Parse(numbers[1]);
                }*/

                builder.AddSignalExtendedMultiplexingInfo(uint.Parse(match.Groups[1].Value), match.Groups[2].Value, $"{multiplexorSignal} {multiplexorRanges}");
            }
            else
            {
                m_observer.SignalExtendedMultiplexingError();
            }

            return true;
        }
    }
}
