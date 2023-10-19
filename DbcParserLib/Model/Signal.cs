using System;
using System.Collections.Generic;

namespace DbcParserLib.Model
{
    internal class ImmutableSignal
    {
        public uint ID { get; }
        public string Name { get; }
        public ushort StartBit { get; }
        public ushort Length { get; }
        public byte ByteOrder { get; }
        [Obsolete("Please use ValueType instead. IsSigned will be removed in future releases")]
        public byte IsSigned { get; }
        public DbcValueType ValueType { get; }
        
        public double InitialValue { get; }
        public double Factor { get; }
        public bool IsInteger { get; }
        public double Offset { get; }
        public double Minimum { get; }
        public double Maximum { get; }
        public string Unit { get; }
        public string[] Receiver { get; }
        [Obsolete("Please use ValueTableMap instead. ValueTable will be removed in future releases")]
        public string ValueTable { get; }
        public IReadOnlyDictionary<int, string> ValueTableMap { get; }
        public string Comment { get; }
        public string Multiplexing { get; }
        public IReadOnlyDictionary<string, CustomProperty> CustomProperties { get; }

        internal ImmutableSignal(Signal signal) 
        {
            ID = signal.ID;
            Name = signal.Name;
            StartBit = signal.StartBit;
            Length = signal.Length;
            ByteOrder = signal.ByteOrder;
            IsSigned = signal.IsSigned;
            ValueType = signal.ValueType;
            InitialValue = signal.InitialValue;
            Factor = signal.Factor;
            IsInteger = signal.IsInteger;
            Offset = signal.Offset;
            Minimum = signal.Minimum;
            Maximum = signal.Maximum;
            Unit = signal.Unit;
            Receiver = signal.Receiver;
            ValueTable = signal.ValueTable;
            ValueTableMap = signal.ValueTableMap;
            Comment = signal.Comment;
            Multiplexing = signal.Multiplexing;
            //TODO: remove explicit cast (CustomProperty in Signal class should be Dictionary instead IDictionary)
            CustomProperties = (IReadOnlyDictionary<string, CustomProperty>)signal.CustomProperties;
        }
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

        internal ImmutableSignal CreateSignal()
        {
            return new ImmutableSignal(this);
        }
    }

    public enum DbcValueType
    {
        Signed, Unsigned, IEEEFloat, IEEEDouble
    }
}
