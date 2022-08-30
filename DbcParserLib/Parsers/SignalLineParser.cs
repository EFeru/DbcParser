using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

namespace DbcParserLib.Parsers
{
    public class SignalLineParser : ILineParser
    {
        private delegate void ParsingStrategy(string line, IDbcBuilder builder);

        private const string SignalLineStarter = "SG_";
        private const string SignedSymbol = "-";
        private static readonly string[] m_commaSeparator = new string[] { Helpers.Comma };
        private static readonly string[] m_commaSpaceSeparator = new string[] { Helpers.Space, Helpers.Comma };
        private static readonly string[] m_signalLineSplittingItems = new string[] { Helpers.Space, "|", "@", "(", ")", "[", "|", "]" };
        private const string SignalRegex = @"\s*SG_\s+([\w]+)\s*([Mm\d]*)\s*:\s*(\d+)\|(\d+)@([01])([+-])\s+\(([\d\+\-eE.]+),([\d\+\-eE.]+)\)\s+\[([\d\+\-eE.]+)\|([\d\+\-eE.]+)\]\s+""(.*)""\s+([\w\s,]+)";

        private readonly ParsingStrategy m_parsingStrategy;

        public SignalLineParser() 
            : this(false)
        { }

        public SignalLineParser(bool withRegex) 
        {
            m_parsingStrategy = withRegex ? (ParsingStrategy)AddSignalRegex : AddSignal;
        }

        public bool TryParse(string line, IDbcBuilder builder)
        {
            if(line.TrimStart().StartsWith(SignalLineStarter) == false)
                return false;

            m_parsingStrategy(line, builder);
            return true;
        }

        private static void AddSignal(string line, IDbcBuilder builder)
        {
            int muxOffset = 0;
            var records = line
                .TrimStart()
                .Split(m_signalLineSplittingItems, StringSplitOptions.RemoveEmptyEntries);

            if (records.Length < 10)
                return;

            var sig = new Signal();
            if (records[2] != ":")    // signal is multiplexed
            {
                muxOffset = 1;
                sig.Multiplexing = records[2];
            }

            sig.Name        = records[1];
            sig.StartBit    = byte.Parse(records[3 + muxOffset], CultureInfo.InvariantCulture);
            sig.Length      = byte.Parse(records[4 + muxOffset], CultureInfo.InvariantCulture);
            sig.ByteOrder   = byte.Parse(records[5 + muxOffset].Substring(0, 1), CultureInfo.InvariantCulture);   // 0 = MSB (Motorola), 1 = LSB (Intel)
            sig.IsSigned    = (byte)(records[5 + muxOffset][1] == '+' ? 0 : 1);

            sig.Factor      = double.Parse(records[6 + muxOffset].Split(m_commaSeparator, StringSplitOptions.None)[0], CultureInfo.InvariantCulture);
            sig.Offset      = double.Parse(records[6 + muxOffset].Split(m_commaSeparator, StringSplitOptions.None)[1], CultureInfo.InvariantCulture);
            sig.Minimum     = double.Parse(records[7 + muxOffset], CultureInfo.InvariantCulture);
            sig.Maximum     = double.Parse(records[8 + muxOffset], CultureInfo.InvariantCulture);
            sig.Unit        = records[9 + muxOffset].Split(new string[] { "\"" }, StringSplitOptions.None)[1];
            sig.Receiver    = string.Join(Helpers.Space, records.Skip(10 + muxOffset)).Split(m_commaSpaceSeparator, StringSplitOptions.RemoveEmptyEntries);  // can be multiple receivers splitted by ","

            builder.AddSignal(sig);
        }

        /// <summary>
        /// This method parses using Regex instead of string split. Beside benchmarking (which may not be the main reason), 
        /// I find that this allows much better control over syntax, create way less arrays and strings, is more robust over spaces etc
        /// </summary>
        /// <param name="line">The line to be parsed</param>
        /// <param name="builder">The dbc builder to be used</param>
        private static void AddSignalRegex(string line, IDbcBuilder builder)
        {
            var match = Regex.Match(line, SignalRegex);

            if (match.Success == false)
                return;

            var sig = new Signal
            {
                Multiplexing = match.Groups[2].Value,
                Name = match.Groups[1].Value,
                StartBit = byte.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture),
                Length = byte.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture),
                ByteOrder = byte.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture),   // 0 = MSB (Motorola), 1 = LSB (Intel)
                IsSigned = (byte)(match.Groups[6].Value == SignedSymbol ? 1 : 0),
                Factor = double.Parse(match.Groups[7].Value, CultureInfo.InvariantCulture),
                Offset = double.Parse(match.Groups[8].Value, CultureInfo.InvariantCulture),
                Minimum = double.Parse(match.Groups[9].Value, CultureInfo.InvariantCulture),
                Maximum = double.Parse(match.Groups[10].Value, CultureInfo.InvariantCulture),
                Unit = match.Groups[11].Value,
                Receiver = match.Groups[12].Value.Split(m_commaSpaceSeparator, StringSplitOptions.RemoveEmptyEntries)  // can be multiple receivers splitted by ","
            };

            builder.AddSignal(sig);
        }
    }
}