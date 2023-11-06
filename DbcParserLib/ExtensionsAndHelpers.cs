using System;
using System.Linq;
using System.Collections.Generic;
using DbcParserLib.Model;
using System.Text;

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

    }

    internal class StringToDictionaryParser
    {
        private IDictionary<int, string> m_dictionary;
        private int m_currentIndex;

        private StringToDictionaryParser(IDictionary<int, string> dict)
        {
            m_dictionary = dict;
        }
        
        public static bool ParseString(string text, out IReadOnlyDictionary<int, string> dictionary)
        {
            dictionary = null;
            var internalDictionary = new Dictionary<int, string>();
            var parser = new StringToDictionaryParser(internalDictionary);
            if (parser.ParseKey(text))
            {
                dictionary = internalDictionary;
                return true;
            }
            return false;
        }
        
        private bool ParseKey(string text)
        {
            var index = text.IndexOf("\"", m_currentIndex, StringComparison.InvariantCulture);
            if(index == -1)
                return true;

            var key = text.Substring(m_currentIndex, index - m_currentIndex);
            m_currentIndex = index + 1;
            return int.TryParse(key, out var intKey) ? ParseValue(text, intKey) : false;
        }

        private bool ParseValue(string text, int key)
        {
            var index = text.IndexOf("\"", m_currentIndex, StringComparison.InvariantCulture);
            if (index == -1)
                return false;

            var value = text.Substring(m_currentIndex, index - m_currentIndex);

            m_dictionary[key] = value;
            m_currentIndex = index +1;
            return ParseKey(text);
        }
    }
}