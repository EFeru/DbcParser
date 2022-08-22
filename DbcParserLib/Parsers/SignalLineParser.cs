using System;
using System.Globalization;

namespace DbcParserLib.Parsers
{
    public class SignalLineParser : ILineParser
    {
        private const string SignalLineStarter = "SG_";

        public bool TryParse(string line, DbcBuilder builder)
        {
            if(line.TrimStart().StartsWith(SignalLineStarter) == false)
                return false;

            AddSignal(line, builder);
            return true;
        }

        private void AddSignal(string line, DbcBuilder builder)
        {
            Signal sig = new Signal();
            int mux = 0;
            string[] records = line.Split(new string[] { " ", "|", "@", "(", ")", "[", "|", "]" }, StringSplitOptions.RemoveEmptyEntries);

            if (records[2] != ":")    // signal is multiplexed
            {
                mux = 1;
                sig.Multiplexing = records[2];
            }

            sig.Name        = records[1];
            sig.StartBit    = byte.Parse(records[3 + mux], CultureInfo.InvariantCulture);
            sig.Length      = byte.Parse(records[4 + mux], CultureInfo.InvariantCulture);
            sig.ByteOrder   = byte.Parse(records[5 + mux].Substring(0, 1), CultureInfo.InvariantCulture);   // 0 = MSB (Motorola), 1 = LSB (Intel)
            if (records[5 + mux].Substring(1, 1) == "+")
                sig.IsSigned = 0;
            else
                sig.IsSigned = 1;

            sig.Factor      = double.Parse(records[6 + mux].Split(new string[] { "," }, StringSplitOptions.None)[0], CultureInfo.InvariantCulture);
            sig.Offset      = double.Parse(records[6 + mux].Split(new string[] { "," }, StringSplitOptions.None)[1], CultureInfo.InvariantCulture);
            sig.Minimum     = double.Parse(records[7 + mux], CultureInfo.InvariantCulture);
            sig.Maximum     = double.Parse(records[8 + mux], CultureInfo.InvariantCulture);
            sig.Unit        = records[9 + mux].Split(new string[] { "\"" }, StringSplitOptions.None)[1];
            sig.Receiver    = records[10 + mux].Split(new string[] { "," }, StringSplitOptions.None);  // can be multiple receivers splitted by ","

            builder.AddSignal(sig);
        }
    }
}