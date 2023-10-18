using System.IO;
using System.Linq;
using System.Collections.Generic;
using DbcParserLib.Model;
using System;

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
        
        internal static ulong BitMask(this Signal signal)
        {
            return (ulong.MaxValue >> (64 - signal.Length));
        }

        [Obsolete("Please use ValueTableMap instead. ToPairs() and ValueTable will be removed in future releases")]
        public static IEnumerable<KeyValuePair<int, string>> ToPairs(this Signal signal)
        {
            return signal.ValueTableMap;
        }

        private const string MultiplexorLabel = "M";
        private const string MultiplexedLabel = "m";

        public static MultiplexingInfo MultiplexingInfo(this Signal signal)
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

        public static bool IsMultiplexed(this Message message)
        {
            return message.Signals.Any(s => s.MultiplexingInfo().Role == MultiplexingRole.Multiplexor);
        }

        internal static void AdjustExtendedId(this Message message)
        {
            // For extended ID bit 31 is always 1
            if(message.ID >= 0x80000000)
            {
                message.IsExtID = true;
                message.ID -= 0x80000000;
            }
        }

        internal static IReadOnlyDictionary<int, string> ToDictionary(this string records)
        {
            var dict = new Dictionary<int, string>();

            if (string.IsNullOrWhiteSpace(records))
                return dict;

            using (var reader = new StringReader(records))
            {
                while (reader.Peek() > -1)
                {
                    var line = reader.ReadLine();
                    
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    
                    // Add duplicated key control and act (eg. strict -> break, warning -> keep going and log, silent-> keep going)
                    var tokens = line.Split(' ');
                    dict[int.Parse(tokens[0])] = tokens[1];
                }
            }
            return dict;
        }

        public static bool CycleTime(this Message message, out int cycleTime)
        {
            if (message.CustomProperties.TryGetValue("GenMsgCycleTime", out var property))
            {
                cycleTime = property.IntegerCustomProperty.Value;
                return true;
            }
            else
            {
                cycleTime = 0;
                return false;
            }
        }
    }
}