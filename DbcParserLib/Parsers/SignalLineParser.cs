using System;
using System.Text.RegularExpressions;
using System.Globalization;
using DbcParserLib.Model;
using DbcParserLib.Observers;

namespace DbcParserLib.Parsers
{
    internal class SignalLineParser : ILineParser
    {
        private const string SignalLineStarter = "SG_ ";
        private const string SignedSymbol = "-";
        private static readonly string[] m_commaSpaceSeparator = new string[] { Helpers.Space, Helpers.Comma };
        private const string SignalRegex = @"\s*SG_\s+([\w]+)\s*([Mm\d]*)\s*:\s*(\d+)\|(\d+)@([01])([+-])\s+\(([\d\+\-eE.]+),([\d\+\-eE.]+)\)\s+\[([\d\+\-eE.]+)\|([\d\+\-eE.]+)\]\s+""(.*)""\s+([\w\s,]+)";

        private readonly IParseFailureObserver m_observer;

        public SignalLineParser(IParseFailureObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            if (line.TrimStart().StartsWith(SignalLineStarter) == false)
                return false;

            var match = Regex.Match(line, SignalRegex);
            if (match.Success)
            {
                var factorStr = match.Groups[7].Value;

                var sig = new Signal
                {
                    ParsingMultiplexing = new ParsingMultiplexing(match.Groups[2].Value, m_observer),
                    Name = match.Groups[1].Value,
                    StartBit = ushort.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture),
                    Length = ushort.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture),
                    ByteOrder = byte.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture),   // 0 = MSB (Motorola), 1 = LSB (Intel)
                    ValueType = (match.Groups[6].Value == SignedSymbol ? DbcValueType.Signed : DbcValueType.Unsigned),
                    IsInteger = IsInteger(factorStr),
                    Factor = double.Parse(match.Groups[7].Value, CultureInfo.InvariantCulture),
                    Offset = double.Parse(match.Groups[8].Value, CultureInfo.InvariantCulture),
                    Minimum = double.Parse(match.Groups[9].Value, CultureInfo.InvariantCulture),
                    Maximum = double.Parse(match.Groups[10].Value, CultureInfo.InvariantCulture),
                    Unit = match.Groups[11].Value,
                    Receiver = match.Groups[12].Value.Split(m_commaSpaceSeparator, StringSplitOptions.RemoveEmptyEntries)  // can be multiple receivers splitted by ","
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
