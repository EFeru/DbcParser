using System;
using System.Collections.Generic;

namespace DbcParserLib.Model
{
    public class Node
    {
        public string Name;
        public string Comment;
        public readonly IDictionary<string, CustomProperty> CustomProperties = new Dictionary<string, CustomProperty>();
    }

    public class Message
    {
        public uint ID;
        public bool IsExtID;
        public string Name;
        public ushort DLC;
        public string Transmitter;
        public string Comment;
        public int CycleTime;
        public List<Signal> Signals = new List<Signal>();
        public readonly IDictionary<string, CustomProperty> CustomProperties = new Dictionary<string, CustomProperty>();
    }

    public class Signal
    {
        private DbcValueType m_ValueType = DbcValueType.Signed;

        public uint ID;
        public string Name;
        public ushort StartBit;
        public ushort Length;
        public byte ByteOrder = 1;
        [Obsolete("Please use ValueType instead. IsSigned will be removed in future releases")]
        public byte IsSigned { get; private set; } = 1;
        public DbcValueType ValueType
        {
            get
            {
                return m_ValueType;
            }
            set
            {
                m_ValueType = value;
                IsSigned = (byte)(value == DbcValueType.Unsigned ? 0 : 1);
            }
        }
        public double InitialValue;
        public double Factor = 1;
        public bool IsInteger = false;
        public double Offset;
        public double Minimum;
        public double Maximum;
        public string Unit;
        public string[] Receiver;
        [Obsolete("Please use ValueTableMap instead. ValueTable will be removed in future releases")]
        public string ValueTable { get; private set; }
        public IReadOnlyDictionary<int, string> ValueTableMap { get; private set; }
        public string Comment;
        public string Multiplexing;
        public readonly IDictionary<string, CustomProperty> CustomProperties = new Dictionary<string, CustomProperty>();

        internal void SetValueTable(IReadOnlyDictionary<int, string> dictValues, string stringValues)
        {
            ValueTableMap = dictValues;
            ValueTable = stringValues;
        }
    }

    public class CustomProperty
    {
        public string Name { get; set; }
        public DbcDataType DataType { get; set; }
        public IntegerCustomProperty IntegerCustomProperty { get; set; }
        public HexCustomProperty HexCustomProperty { get; set; }
        public FloatCustomProperty FloatCustomProperty { get; set; }
        public StringCustomProperty StringCustomProperty { get; set; }
        public EnumCustomProperty EnumCustomProperty { get; set; }
    }

    public class IntegerCustomProperty
    {
        public int Maximum { get; set; }
        public int Minimum { get; set; }
        public int Default { get; set; }
        public  int Value { get; set; }
    }

    public class HexCustomProperty
    {
        public int Maximum { get; set; }
        public int Minimum { get; set; }
        public int Default { get; set; }
        public int Value { get; set; }
    }

    public class FloatCustomProperty
    {
        public double Maximum { get; set; }
        public double Minimum { get; set; }
        public double Default { get; set; }
        public double Value { get; set; }
    }
    public class StringCustomProperty
    {
        public string Default { get; set; }
        public string Value { get; set; }
    }

    public class EnumCustomProperty
    {
        public string[] Default { get; set; }
        public string[] Value { get; set; }
    }

    public enum DbcObjectType 
    {
        Node, Message, Signal, Environment
    }

    public enum DbcDataType
    {
        Integer, Hex, Float, String, Enum
    }

    public enum DbcValueType
    {
        Signed, Unsigned, IEEEFloat, IEEEDouble
    }
}