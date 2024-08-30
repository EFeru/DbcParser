using DbcParserLib.Observers;
using System;
using System.Collections.Generic;

namespace DbcParserLib.Model
{
    public class ParsingMultiplexing
    {
        public MultiplexingRole Role { get; }
        public string MultiplexorSignal { get; private set; }

        //Initialize so that FirstOrDefault wouldn't crash when converting to current MultiplexingInfo
        public ISet<uint> MultiplexerValues { get; } = new HashSet<uint>(); 

        private const string MultiplexorLabel = "M";
        private const string MultiplexedLabel = "m";

        private bool valid = true;
        private bool hasExtendedInfo = false;
        private readonly IParseFailureObserver m_parseFailureObserver;

        public ParsingMultiplexing(string signalMultiplexingString, IParseFailureObserver parseFailureObserver)
        {
            m_parseFailureObserver = parseFailureObserver;

            if (string.IsNullOrWhiteSpace(signalMultiplexingString))
            {
                Role = MultiplexingRole.None;
                return;
            }

            if (signalMultiplexingString.Equals(MultiplexorLabel))
            {
                Role = MultiplexingRole.Multiplexor;
                return;
            }

            if (signalMultiplexingString.StartsWith(MultiplexedLabel))
            {
                var isMultiplexedMultiplexor = signalMultiplexingString.EndsWith(MultiplexorLabel);
                var substringLength = signalMultiplexingString.Length - (isMultiplexedMultiplexor ? 2 : 1);

                if (uint.TryParse(signalMultiplexingString.Substring(1, substringLength), out var multiplexerValue))
                {
                    MultiplexerValues.Add(multiplexerValue);
                }
                else 
                {
                    valid = false;
                    m_parseFailureObserver.SignalMultiplexingSyntaxError();
                    return;
                }

                if (isMultiplexedMultiplexor)
                {
                    Role = MultiplexingRole.Multiplexed | MultiplexingRole.Multiplexor;
                    return;
                }
                Role = MultiplexingRole.Multiplexed;
                return;
            }

            valid = false;
            m_parseFailureObserver.SignalMultiplexingSyntaxError();
        }

        public void AddExtendedMultiplexingInformation(string multiplexorSignal, string ranges)
        {
            if (valid == false)
            {
                m_parseFailureObserver.SignalExtendedMultiplexingSyntaxError();
                return;
            }

            var rangesArray = ranges.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var range in rangesArray)
            {
                var rangeClean = range.Trim();
                var numbers = rangeClean.Split('-');

                var lowerParsed = uint.TryParse(numbers[0], out var lower);
                var upperParsed = uint.TryParse(numbers[1], out var upper);

                if (lowerParsed == true && upperParsed == true && lower <= upper)
                {
                    for (var i = lower; i <= upper; i++)
                    {
                        MultiplexerValues.Add(i);
                    }
                }
                else 
                {
                    m_parseFailureObserver.SignalExtendedMultiplexingSyntaxError();
                    return;
                }
            }

            MultiplexorSignal = multiplexorSignal;
            hasExtendedInfo = true;
        }
    }
}
