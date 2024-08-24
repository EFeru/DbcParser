using System.Collections.Generic;

namespace DbcParserLib.Model;

public class Signal
{
    public uint MessageID { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public ushort StartBit { get; internal set; }
    public ushort Length { get; internal set; }
    public byte ByteOrder { get; internal set; } = 1;
    public DbcValueType ValueType { get; internal set; } = DbcValueType.Signed;
    public bool IsInteger { get; private set; } 
    public double Factor { get; internal set; } = 1;
    public double Offset { get; internal set; }
    public bool HasScaling { get; private set; } 
    public double Minimum { get; internal set; }
    public double Maximum { get; internal set; }
    public bool HasLimits { get; private set; }
    public string Unit { get; internal set; } = string.Empty;
    public string[] Receiver { get; internal set; } = [];
    public IReadOnlyDictionary<int, string> ValueTableMap { get; internal set; } = new Dictionary<int, string>();
    public string Comment { get; internal set; } = string.Empty;
    public MultiplexingInfo MultiplexingInfo { get; private set; } = new();
    internal string m_multiplexing = string.Empty;
    internal Multiplexing? m_extendedMultiplex;
    public IReadOnlyDictionary<string, CustomProperty> CustomProperties => m_customProperties;
    internal readonly Dictionary<string, CustomProperty> m_customProperties = new();
    public double? InitialValue { get; private set; }

    internal void FinishUp(Message message, bool messageHasComplexMultiplexing)
    {
        InitialValue = null;
        var hasInitialValue = TryGetInitialValue(out var initialValue);
        InitialValue = hasInitialValue ? initialValue : null;
        MultiplexingInfo = new MultiplexingInfo(this, message, messageHasComplexMultiplexing);
        HasScaling = ExtensionsAndHelpers.IsDoubleZero(Offset) && ExtensionsAndHelpers.AreDoublesEqual(Factor, 1.0);
        HasLimits = !ExtensionsAndHelpers.IsDoubleZero(Minimum) || !ExtensionsAndHelpers.IsDoubleZero(Maximum);
        IsInteger = CheckIsInteger();
    }

    private bool TryGetInitialValue(out double? initialValue)
    {
        initialValue = null;

        if (!m_customProperties.TryGetValue("GenSigStartValue", out var property))
        {
            return false;
        }

        double value;
        switch (property.PropertyValue)
        {
            case FloatPropertyValue floatPropertyValue:
                value = floatPropertyValue.Value;
                break;
            case HexPropertyValue hexPropertyValue:
                value = hexPropertyValue.Value;
                break;
            case IntegerPropertyValue integerPropertyValue:
                value = integerPropertyValue.Value;
                break;
            default:
                return false;
        }

        initialValue = value * Factor + Offset;
        return true;
    }
    
    private bool CheckIsInteger()
    {
        if (ValueType is not (DbcValueType.Signed or DbcValueType.Unsigned))
        {
            return false;
        }

        return ExtensionsAndHelpers.IsWholeNumber(Factor) && ExtensionsAndHelpers.IsWholeNumber(Offset);
    }
}

public enum DbcValueType
{
    Signed,
    Unsigned,
    IEEEFloat,
    IEEEDouble
}