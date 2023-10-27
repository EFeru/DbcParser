using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        
        internal static ulong BitMask(this Signal signal)
        {
            return (ulong.MaxValue >> (64 - signal.Length));
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
            const string valueTableValueRegex = @"(-?\d+)\s+(""[^""]*"")";
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
                    
                    var match = Regex.Match(line, valueTableValueRegex);
                    if (match.Success)
                        dict[int.Parse(match.Groups[1].Value)] = match.Groups[2].Value;
                }
            }
            return dict;
        }

        public static bool CycleTime(this Message message, out int cycleTime)
        {
            cycleTime = 0;

            if (message.CustomProperties.TryGetValue("GenMsgCycleTime", out var property))
            {
                cycleTime = property.IntegerCustomProperty.Value;
                return true;
            }
            else
                return false;
        }

        internal static bool InitialValue(this Signal signal, out double initialValue)
        {
            initialValue = 0;

            if (signal.CustomProperties.TryGetValue("GenSigStartValue", out var property))
            {
                double value = 0;
                switch (property.CustomPropertyDefinition.DataType)
                {
                    case CustomPropertyDataType.Float:
                        value = property.FloatCustomProperty.Value;
                        break;
                    case CustomPropertyDataType.Hex:
                        value = property.HexCustomProperty.Value;
                        break;
                    case CustomPropertyDataType.Integer:
                        value = property.IntegerCustomProperty.Value;
                        break;
                    default:
                        return false;
                }

                initialValue = value * signal.Factor + signal.Offset;
                return true;
            }
            else
                return false;
        }
    }
}