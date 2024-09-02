using System;
using System.Linq;
using System.Collections.Generic;
using DbcParserLib.Model;
using System.Text.RegularExpressions;

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

        internal static bool TryParseToDict(this string records, out IReadOnlyDictionary<int, string> dict)
        {
            return StringToDictionaryParser.ParseString(records, out dict);
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

        private static readonly string[] SpaceArray = { " " };
        public static readonly string[] CommaSpaceSeparator = { " ", "," };

        public static string[] SplitBySpace(this string value)
        {
            return value.Split(SpaceArray, System.StringSplitOptions.RemoveEmptyEntries);
        }

        // Sequence of return codes was taken from the internals of "String.ReplaceLineEndings" method.
        private const string NewLineChars = "\r\f\u0085\u2028\u2029\n";
        private static readonly string pattern = $"[{Regex.Escape(NewLineChars)}]+";

        public static string ReplaceNewlinesWithSpace(this string input)
        {
            // Would like to use "String.ReplaceLineEndings" but its unavailable because of the target frameworks
            // Feel free to optimate
            return Regex.Replace(input, pattern, " ");
        }
    }

    internal class StringToDictionaryParser
    {
        private readonly IDictionary<int, string> m_dictionary;

        private StringToDictionaryParser(IDictionary<int, string> dict)
        {
            m_dictionary = dict;
        }
        
        public static bool ParseString(string text, out IReadOnlyDictionary<int, string> dictionary)
        {
            dictionary = null;
            var internalDictionary = new Dictionary<int, string>();
            var parser = new StringToDictionaryParser(internalDictionary);
            if (parser.ParseKey(text, 0))
            {
                dictionary = internalDictionary;
                return true;
            }
            return false;
        }
        
        private bool ParseKey(string text, int offset)
        {
            var index = text.IndexOf(Helpers.DoubleQuotes, offset, StringComparison.InvariantCulture);
            if(index == -1)
                return true;

            var key = text.Substring(offset, index - offset);
            offset = index + 1;
            return int.TryParse(key, out var intKey) && ParseValue(text, offset, intKey);
        }

        private bool ParseValue(string text, int offset, int key)
        {
            var index = text.IndexOf(Helpers.DoubleQuotes, offset, StringComparison.InvariantCulture);
            if (index == -1)
                return false;

            var value = text.Substring(offset, index - offset);

            m_dictionary[key] = value;
            offset = index +1;
            return ParseKey(text, offset);
        }
    }
}