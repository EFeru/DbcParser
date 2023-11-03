using System;
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
            var stateMachine = new StringToDictionaryStateMachine();
            return stateMachine.ParseString(records, out dict);
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

    internal class StringToDictionaryStateMachine
    {
        private readonly Func<string, bool>[,] m_fsm;
        private Dictionary<int, string> m_dictionary = new Dictionary<int, string>();
        private int m_currentKey;

        public StringToDictionaryStateMachine()
        {
            m_fsm = new Func<string, bool>[2, 4] { 
                //Start       //FoundQuotes   //NotFound  //End, 
                {ParseKey,     ParseValue,    Error,      End       },  //FindingIndex
                {DoNothing,    ParseKey,      Error,      DoNothing },  //FindingValue
            };
        }

        private enum States { FindingIndex, FindingValue };
        private States State { get; set; }
        private enum Events { Start, FoundQuotes, NotFound, End};
        
        public bool ParseString(string text, out IReadOnlyDictionary<int, string> dictionary)
        {
            State = States.FindingIndex;
            dictionary = m_dictionary = new Dictionary<int, string>();
            return ProcessEvent(text, Events.Start);
        }
        
        private bool ParseKey(string text)
        {
            State = States.FindingIndex;

            var splitIndex = text.IndexOf("\"", StringComparison.InvariantCulture);
            if(splitIndex < 0)
                return ProcessEvent(text, Events.End);

            var index = text.Substring(0, splitIndex);
            text = text.Substring(splitIndex + 1);
            return ProcessEvent(text, int.TryParse(index, out m_currentKey) ? Events.FoundQuotes : Events.NotFound);
        }

        private bool ParseValue(string text)
        {
            State = States.FindingValue;
            
            var splitIndex = text.IndexOf("\"", StringComparison.InvariantCulture);
            if (splitIndex == -1)
                return ProcessEvent(text, Events.NotFound);

            var value = text.Substring(0, splitIndex);
            text = text.Substring(splitIndex + 1);
            m_dictionary[m_currentKey] = value;
            return ProcessEvent(text, Events.FoundQuotes);
        }

        private bool ProcessEvent(string text, Events theEvent)
        {
            return m_fsm[(int)State, (int)theEvent].Invoke(text);
        }

        private static bool Error(string text)
        {
            return false;
        }

        private static bool End(string text)
        {
            return true;
        }

        private static bool DoNothing(string text)
        {
            return true;
        }
    }
}