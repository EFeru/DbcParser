using DbcParserLib.Observers;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DbcParserLib.Parsers
{
    internal class ExtendedMultiplexingLineParser : ILineParser
    {
        private const string MessageIdGroup = "MessageId";
        private const string SignalGroup = "Signal";
        private const string MultiplexorSignalGroup = "MultiplexorSignal";
        private const string MultiplexValueRangesGroup = "MultiplexValueRanges";

        private const string SignalLineStarter = "SG_MUL_VAL_ ";
        private readonly string ExtendedMultiplexingRegex = $@"SG_MUL_VAL_\s+(?<{MessageIdGroup}>\d+)\s+(?<{SignalGroup}>\S+)\s+(?<{MultiplexorSignalGroup}>\S+)\s+(?<{MultiplexValueRangesGroup}>(?:\d+-\d+,?\s*)+);?";

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

            var match = Regex.Match(line, ExtendedMultiplexingRegex);

            if (match.Success)
            {
                var messageId = uint.Parse(match.Groups[MessageIdGroup].Value);
                var signal = match.Groups[SignalGroup].Value;
                var multiplexorSignal = match.Groups[MultiplexorSignalGroup].Value;
                var multiplexorRanges = match.Groups[MultiplexValueRangesGroup].Value;

                var rangesArray = multiplexorRanges.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var parsedRanges = new List<Tuple<uint, uint>>();

                foreach (var range in rangesArray)
                {
                    var rangeClean = range.Trim();
                    var numbers = rangeClean.Split('-');

                    var lowerParsed = uint.TryParse(numbers[0], out var lower);
                    var upperParsed = uint.TryParse(numbers[1], out var upper);

                    if (lowerParsed == true && upperParsed == true && lower <= upper)
                    {
                        parsedRanges.Add(new Tuple<uint, uint>(lower, upper));
                    }
                    else
                    {
                        m_observer.SignalExtendedMultiplexingSyntaxError();
                        return true;
                    }
                }

                builder.AddSignalExtendedMultiplexingInfo(messageId, signal, multiplexorSignal, parsedRanges);
            }
            else
            {
                m_observer.SignalExtendedMultiplexingSyntaxError();
            }

            return true;
        }
    }
}
