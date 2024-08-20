using System.Globalization;
using System.Linq;
using DbcParserLib.Observers;

namespace DbcParserLib.Model;

public class CustomPropertyDefinition
{
    private readonly IParseFailureObserver observer;
    public string Name { get; internal set; }
    public CustomPropertyDataType DataType { get; internal set; }
    public NumericCustomPropertyDefinition<int> IntegerCustomProperty { get; internal set; }
    public NumericCustomPropertyDefinition<int> HexCustomProperty { get; internal set; }
    public NumericCustomPropertyDefinition<double> FloatCustomProperty { get; internal set; }
    public StringCustomPropertyDefinition StringCustomProperty { get; internal set; }
    public EnumCustomPropertyDefinition EnumCustomProperty { get; internal set; }

    public CustomPropertyDefinition(IParseFailureObserver observer)
    {
        this.observer = observer;
    }

    public void SetCustomPropertyDefaultValue(string value, bool isNumeric)
    {
        switch (DataType)
        {
            case CustomPropertyDataType.Integer:
                if (!TryGetIntegerValue(value, isNumeric, out var integerValue))
                {
                    return;
                }

                IntegerCustomProperty.Default = integerValue;
                break;

            case CustomPropertyDataType.Hex:
                if (!TryGetHexValue(value, isNumeric, out var hexValue))
                {
                    return;
                }

                HexCustomProperty.Default = hexValue;
                break;

            case CustomPropertyDataType.Float:
                if (!TryGetFloatValue(value, isNumeric, out var floatValue))
                {
                    return;
                }

                FloatCustomProperty.Default = floatValue;
                break;

            case CustomPropertyDataType.String:
                if (!IsString(isNumeric))
                {
                    return;
                }

                StringCustomProperty.Default = value;
                break;

            case CustomPropertyDataType.Enum:
                if (!TryGetEnumValue(value, isNumeric, out var enumValue))
                {
                    return;
                }

                EnumCustomProperty.Default = enumValue;
                break;
        }
    }

    internal bool TryGetIntegerValue(string value, bool isNumeric, out int integerValue)
    {
        integerValue = 0;
        if (!isNumeric)
        {
            observer.PropertySyntaxError();
            return false;
        }

        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture,
                out integerValue))
        {
            observer.PropertySyntaxError();
            return false;
        }

        if (CanAcceptAllValue(CustomPropertyDataType.Integer))
        {
            return true;
        }

        if (integerValue < IntegerCustomProperty.Minimum || integerValue > IntegerCustomProperty.Maximum)
        {
            observer.PropertyValueOutOfBound(Name, value);
            return false;
        }

        return true;
    }

    internal bool TryGetHexValue(string value, bool isNumeric, out int hexValue)
    {
        hexValue = 0;
        if (!isNumeric)
        {
            observer.PropertySyntaxError();
            return false;
        }

        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture,
                out hexValue))
        {
            observer.PropertySyntaxError();
            return false;
        }

        if (CanAcceptAllValue(CustomPropertyDataType.Hex))
        {
            return true;
        }

        if (hexValue < HexCustomProperty.Minimum || hexValue > HexCustomProperty.Maximum)
        {
            observer.PropertyValueOutOfBound(Name, value);
            return false;
        }

        return true;
    }

    internal bool TryGetFloatValue(string value, bool isNumeric, out float floatValue)
    {
        floatValue = 0;
        if (!isNumeric)
        {
            observer.PropertySyntaxError();
            return false;
        }

        if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture,
                out floatValue))
        {
            observer.PropertySyntaxError();
            return false;
        }

        if (CanAcceptAllValue(CustomPropertyDataType.Float))
        {
            return true;
        }

        if (floatValue < FloatCustomProperty.Minimum || floatValue > FloatCustomProperty.Maximum)
        {
            observer.PropertyValueOutOfBound(Name, value);
            return false;
        }

        return true;
    }

    internal bool IsString(bool isNumeric)
    {
        if (isNumeric)
        {
            observer.PropertySyntaxError();
            return false;
        }

        return true;
    }

    internal bool TryGetEnumValue(string value, bool isNumeric, out string enumValue)
    {
        enumValue = null;
        if (isNumeric)
        {
            if (!TryGetEnumValueFromIndex(value, out enumValue))
            {
                return false;
            }
        }
        else
        {
            if (!EnumCustomProperty.Values.Contains(value))
            {
                observer.PropertyValueOutOfBound(Name, value);
                return false;
            }

            enumValue = value;
        }

        return true;
    }

    private bool TryGetEnumValueFromIndex(string value, out string enumValue)
    {
        enumValue = null;

        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
        {
            observer.PropertySyntaxError();
            return false;
        }

        if (index < 0 || index >= EnumCustomProperty.Values.Length)
        {
            observer.PropertyValueOutOfIndex(Name, value);
            return false;
        }

        enumValue = EnumCustomProperty.Values[index];
        return true;
    }

    private bool CanAcceptAllValue(CustomPropertyDataType dataType)
    {
        switch (dataType)
        {
            case CustomPropertyDataType.Integer:
                return IntegerCustomProperty.Minimum == 0 && IntegerCustomProperty.Maximum == 0;
            case CustomPropertyDataType.Hex:
                return HexCustomProperty.Minimum == 0 && HexCustomProperty.Maximum == 0;
            case CustomPropertyDataType.Float:
                return ExtensionsAndHelpers.AreDoublesEqual(FloatCustomProperty.Minimum, 0) && ExtensionsAndHelpers.AreDoublesEqual(FloatCustomProperty.Maximum, 0);
            case CustomPropertyDataType.String:
            case CustomPropertyDataType.Enum:
            default:
                return false;
        }
    }
}

public class NumericCustomPropertyDefinition<T>
{
    public T Maximum { get; internal set; }
    public T Minimum { get; internal set; }
    public T Default { get; internal set; }
}

public class StringCustomPropertyDefinition
{
    public string Default { get; internal set; }
}

public class EnumCustomPropertyDefinition
{
    public string Default { get; internal set; }
    public string[] Values { get; internal set; }
}

public enum CustomPropertyObjectType
{
    Node,
    Message,
    Signal,
    Environment
}

public enum CustomPropertyDataType
{
    Integer,
    Hex,
    Float,
    String,
    Enum
}