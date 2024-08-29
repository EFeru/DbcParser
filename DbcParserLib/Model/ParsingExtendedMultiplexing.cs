using System.Collections.Generic;

namespace DbcParserLib.Model
{
    internal class ParsingExtendedMultiplexing
    {
        public string MultiplexorSignal { get; }
        public List<MultiplexingRange> MultiplexingRanges { get; }

        public ParsingExtendedMultiplexing(string multiplexorSignal, List<MultiplexingRange> multiplexingRanges)
        {
            MultiplexorSignal = multiplexorSignal;
            MultiplexingRanges = multiplexingRanges;
        }
    }
}
