using System;
using System.Text.RegularExpressions;
using System.Globalization;
using DbcParserLib.Model;
using DbcParserLib.Observers;

namespace DbcParserLib.Parsers
{
    internal class SignalLineParser : ILineParser
    {
        private const string NameGroup = "Name";
        private const string MultiplexerGroup = "Multiplexer";
        private const string StartBirGroup = "StartBit";
        private const string SizeGroup = "Size";
        private const string ByteOrderGroup = "ByteOrder";
        private const string ValueTypeGroup = "ValueType";
        private const string FactorGroup = "Factor";
        private const string OffsetGroup = "Offset";
        private const string MinGroup = "Min";
        private const string MaxGroup = "Max";
        private const string UnitGroup = "Unit";
        private const string ReceiverGroup = "Receiver";
        private const string SignalLineStarter = "SG_ ";
        private const string SignedSymbol = "-";
        private static readonly string[] CommaSpaceSeparator = { Helpers.Space, Helpers.Comma };

        private readonly string m_signalRegex = $@"\s*SG_\s+(?<{NameGroup}>[\w]+)\s*(?<{MultiplexerGroup}>[Mm\d]*)\s*:\s*(?<{StartBirGroup}>\d+)\|(?<{SizeGroup}>\d+)@(?<{ByteOrderGroup}>[01])" +
                                                $@"(?<{ValueTypeGroup}>[+-])\s+\((?<{FactorGroup}>[\d\+\-eE.]+),(?<{OffsetGroup}>[\d\+\-eE.]+)\)\s+\[(?<{MinGroup}>[\d\+\-eE.]+)\|(?<{MaxGroup}>[\d\+\-eE.]+)\]" +
                                                $@"\s+""(?<{UnitGroup}>.*)""\s+(?<{ReceiverGroup}>[\w\s,]+)";

        private readonly IParseFailureObserver m_observer;

        public SignalLineParser(IParseFailureObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            if (line.TrimStart().StartsWith(SignalLineStarter) == false)
                return false;

            var match = Regex.Match(line, m_signalRegex);
            if (match.Success)
            {
                var factorStr = match.Groups[FactorGroup].Value;
                var sig = new Signal
                {
                    Multiplexing = match.Groups[MultiplexerGroup].Value,
                    Name = match.Groups[NameGroup].Value,
                    StartBit = ushort.Parse(match.Groups[StartBirGroup].Value, CultureInfo.InvariantCulture),
                    Length = ushort.Parse(match.Groups[SizeGroup].Value, CultureInfo.InvariantCulture),
                    ByteOrder = byte.Parse(match.Groups[ByteOrderGroup].Value, CultureInfo.InvariantCulture),   // 0 = MSB (Motorola), 1 = LSB (Intel)
                    ValueType = (match.Groups[ValueTypeGroup].Value == SignedSymbol ? DbcValueType.Signed : DbcValueType.Unsigned),
                    IsInteger = IsInteger(factorStr),
                    Factor = double.Parse(match.Groups[FactorGroup].Value, CultureInfo.InvariantCulture),
                    Offset = double.Parse(match.Groups[OffsetGroup].Value, CultureInfo.InvariantCulture),
                    Minimum = double.Parse(match.Groups[MinGroup].Value, CultureInfo.InvariantCulture),
                    Maximum = double.Parse(match.Groups[MaxGroup].Value, CultureInfo.InvariantCulture),
                    Unit = match.Groups[UnitGroup].Value,
                    Receiver = match.Groups[ReceiverGroup].Value.Split(CommaSpaceSeparator, StringSplitOptions.RemoveEmptyEntries)  // can be multiple receivers splitted by ","
                };

                builder.AddSignal(sig);
            }
            else
                m_observer.SignalSyntaxError();

            return true;
        }

        private static bool IsInteger(string str)
        {
            return int.TryParse(str, out _);
        }
    }
}
