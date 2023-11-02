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
        public DbcValueType ValueType { get; }
        public double InitialValue { get; }
        public double Factor { get; }
        public bool IsInteger { get; }
        public double Offset { get; }
        public double Minimum { get; }
        public double Maximum { get; }
        public string Unit { get; }
        public string[] Receiver { get; }
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
            ValueType = signal.ValueType;
            InitialValue = signal.InitialValue;
            Factor = signal.Factor;
            IsInteger = signal.IsInteger;
            Offset = signal.Offset;
            Minimum = signal.Minimum;
            Maximum = signal.Maximum;
            Unit = signal.Unit;
            Receiver = signal.Receiver;
            ValueTableMap = signal.ValueTableMap;
            Comment = signal.Comment;
            Multiplexing = signal.Multiplexing;
            CustomProperties = signal.CustomProperties;
        }
    }

    public class Signal
    {
        public uint ID;
        public string Name;
        public ushort StartBit;
        public ushort Length;
        public byte ByteOrder = 1;
        public DbcValueType ValueType = DbcValueType.Signed;
        public double Factor = 1;
        public bool IsInteger = false;
        public double Offset;
        public double Minimum;
        public double Maximum;
        public string Unit;
        public string[] Receiver;
        public IReadOnlyDictionary<int, string> ValueTableMap = new Dictionary<int, string>();
        public string Comment;
        public string Multiplexing;
        public readonly Dictionary<string, CustomProperty> CustomProperties = new Dictionary<string, CustomProperty>();
        public double InitialValue
        {
            get
            {
                this.InitialValue(out var initialValue);
                return initialValue;
            }
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
