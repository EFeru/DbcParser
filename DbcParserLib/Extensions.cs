using System.IO;
using System.Linq;
using System.Collections.Generic;
using DbcParserLib.Model;

namespace DbcParserLib
{
    public static class ExtensionsAndHelpers
    {
        public static bool Motorola(this Signal signal)
        {
            return signal.Msb();
        }

        public static bool Msb(this Signal signal)
        {
            return signal.ByteOrder == 0;
        }

        public static bool Lsb(this Signal signal)
        {
            return signal.ByteOrder != 0;
        }

        public static bool Intel(this Signal signal)
        {
            return signal.Lsb();
        }

        public static IEnumerable<KeyValuePair<int, string>> ToPairs(this Signal signal)
        {
            if (string.IsNullOrWhiteSpace(signal.ValueTable))
                yield break;
            
            using(var reader = new StringReader(signal.ValueTable))
            {
                while(reader.Peek() > -1)
                {
                    var tokens = reader.ReadLine().Split(' ');
                    yield return new KeyValuePair<int, string>(int.Parse(tokens[0]), tokens[1]);
                }
            }
        }


        private const string MultiplexorLabel = "M";
        private const string MultiplexedLabel = "m";

        public static MultiplexingInfo Multiplexing(this Signal signal)
        {
            if(string.IsNullOrWhiteSpace(signal.Multiplexing))
                return new MultiplexingInfo(MultiplexingRole.None);

            if(signal.Multiplexing.Equals(MultiplexorLabel))
                return new MultiplexingInfo(MultiplexingRole.Multiplexor);
            
            if(signal.Multiplexing.StartsWith(MultiplexedLabel))
            {
                var substringLength = signal.Multiplexing.Length - (signal.Multiplexing.EndsWith(MultiplexorLabel) ? 2 : 1);

                if(int.TryParse(signal.Multiplexing.Substring(1, substringLength), out var group))
                    return new MultiplexingInfo(MultiplexingRole.Multiplexed, group);
            }

            return new MultiplexingInfo(MultiplexingRole.Unknown);
        }

        public static bool Multiplexed(this Message message)
        {
            return message.Signals.Any(s => s.Multiplexing().Role == MultiplexingRole.Multiplexor);
        }
    }
}