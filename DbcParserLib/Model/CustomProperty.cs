using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DbcParserLib.Observers;

namespace DbcParserLib.Model;

public class CustomProperty
{
    private readonly IParseFailureObserver observer;
    public string Name { get; }
    public CustomPropertyDataType DataType { get; }
    public ICustomPropertyDefinition PropertyDefinition { get; }
    public ICustomPropertyValue PropertyValue { get; private set; }

    public CustomProperty(string name, CustomPropertyDataType dataType, ICustomPropertyDefinition propertyDefinition, IParseFailureObserver observer)
    {
        this.observer = observer;
        DataType = dataType;
        PropertyDefinition = propertyDefinition;
        Name = name;
        PropertyValue = new PropertyValueUndefined();
    }

    internal CustomProperty Clone()
    {
        // The only thing that actually needs to be cloned is the value as it can differ between different if the same property is used multiple times (e.g. at different nodes)
        var clone = new CustomProperty(Name, DataType, PropertyDefinition, observer);
        switch (PropertyValue)
        {
            case IntegerPropertyValue integerPropertyValue:
                clone.PropertyValue = new IntegerPropertyValue(integerPropertyValue.Value);
                break;
            case HexPropertyValue hexPropertyValue:
                clone.PropertyValue = new HexPropertyValue(hexPropertyValue.Value);
                break;
            case FloatPropertyValue floatPropertyValue:
                clone.PropertyValue = new FloatPropertyValue(floatPropertyValue.Value);
                break;
            case StringPropertyValue stringPropertyValue:
                clone.PropertyValue = new StringPropertyValue(stringPropertyValue.Value);
                break;
            case EnumPropertyValue enumPropertyValue:
                clone.PropertyValue = new EnumPropertyValue(enumPropertyValue.Value);
                break;
        }
        return clone;
    }

    public void SetCustomPropertyDefaultValue(string value, bool isNumeric)
    {
        switch (PropertyDefinition)
        {
            case IntegerCustomPropertyDefinition integerCustomPropertyDefinition:
                if (!ValidateIntegerValue(value, isNumeric, integerCustomPropertyDefinition, out var integerValue))
                {
                    return;
                }

                integerCustomPropertyDefinition.Default = integerValue;
                break;

            case HexCustomPropertyDefinition hexCustomPropertyDefinition:
                if (!ValidateHexValue(value, isNumeric, hexCustomPropertyDefinition, out var hexValue))
                {
                    return;
                }

                hexCustomPropertyDefinition.Default = hexValue;
                break;

            case FloatCustomPropertyDefinition floatCustomPropertyDefinition:
                if (!ValidateFloatValue(value, isNumeric, floatCustomPropertyDefinition, out var floatValue))
                {
                    return;
                }

                floatCustomPropertyDefinition.Default = floatValue;
                break;

            case StringCustomPropertyDefinition stringCustomPropertyDefinition:
                if (!ValidateStringValue(isNumeric))
                {
                    return;
                }

                stringCustomPropertyDefinition.Default = value;
                break;

            case EnumCustomPropertyDefinition enumCustomPropertyDefinition:
                if (!ValidateEnumValue(value, isNumeric, enumCustomPropertyDefinition, out var enumValue))
                {
                    return;
                }

                enumCustomPropertyDefinition.Default = enumValue;
                break;
        }
    }

    public bool SetCustomPropertyValue(string value, bool isNumeric)
    {
        switch (PropertyDefinition)
        {
            case IntegerCustomPropertyDefinition integerCustomPropertyDefinition:
                if (!ValidateIntegerValue(value, isNumeric, integerCustomPropertyDefinition, out var integerValue))
                {
                    return false;
                }

                PropertyValue = new IntegerPropertyValue
                (
                    value : integerValue
                );
                break;

            case HexCustomPropertyDefinition hexCustomPropertyDefinition:
                if (!ValidateHexValue(value, isNumeric, hexCustomPropertyDefinition, out var hexValue))
                {
                    return false;
                }

                PropertyValue = new HexPropertyValue
                (
                    value : hexValue
                );
                break;

            case FloatCustomPropertyDefinition floatCustomPropertyDefinition:
                if (!ValidateFloatValue(value, isNumeric, floatCustomPropertyDefinition, out var floatValue))
                {
                    return false;
                }

                PropertyValue = new FloatPropertyValue
                (
                    value : floatValue
                );
                break;

            case StringCustomPropertyDefinition:
                if (!ValidateStringValue(isNumeric))
                {
                    return false;
                }

                PropertyValue = new StringPropertyValue
                (
                    value : value
                );
                break;

            case EnumCustomPropertyDefinition enumCustomPropertyDefinition:
                if (!ValidateEnumValue(value, isNumeric, enumCustomPropertyDefinition, out var enumValue))
                {
                    return false;
                }

                PropertyValue = new EnumPropertyValue
                (
                    value : enumValue!
                );
                break;
            default:
                return false;
        }

        return true;
    }
    
    public void SetDefaultIfNotSet()
    {
        if (PropertyValue is not PropertyValueUndefined)
        {
            return;
        }
        
        switch (PropertyDefinition)
        {
            case IntegerCustomPropertyDefinition integerCustomPropertyDefinition:
                if (integerCustomPropertyDefinition.Default is null)
                {
                    return;
                }
                PropertyValue = new IntegerPropertyValue
                (
                    value : integerCustomPropertyDefinition.Default.Value
                );
                break;
            case HexCustomPropertyDefinition hexCustomPropertyDefinition:
                if (hexCustomPropertyDefinition.Default is null)
                {
                    return;
                }
                PropertyValue = new HexPropertyValue
                (
                    value : hexCustomPropertyDefinition.Default.Value
                );
                break;
            case FloatCustomPropertyDefinition floatCustomPropertyDefinition:
                if (floatCustomPropertyDefinition.Default is null)
                {
                    return;
                }
                PropertyValue = new FloatPropertyValue
                (
                    value : floatCustomPropertyDefinition.Default.Value
                );
                break;
            case StringCustomPropertyDefinition stringCustomPropertyDefinition:
                if (stringCustomPropertyDefinition.Default is null)
                {
                    return;
                }
                PropertyValue = new StringPropertyValue
                (
                    value : stringCustomPropertyDefinition.Default
                );
                break;
            case EnumCustomPropertyDefinition enumCustomPropertyDefinition:
                if (enumCustomPropertyDefinition.Default is null)
                {
                    return;
                }
                PropertyValue = new EnumPropertyValue
                (
                    value : enumCustomPropertyDefinition.Default
                );
                break;
        }
    }
    
    private bool ValidateIntegerValue(string value, bool isNumeric, IntegerCustomPropertyDefinition propertyDefinition, out int integerValue)
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

        if (DoesPropertyHaveNoLimits())
        {
            return true;
        }

        if (integerValue < propertyDefinition.Minimum || integerValue > propertyDefinition.Maximum)
        {
            observer.PropertyValueOutOfBound(Name, value);
            return false;
        }

        return true;
    }

    private bool ValidateHexValue(string value, bool isNumeric, HexCustomPropertyDefinition propertyDefinition, out int hexValue)
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

        if (DoesPropertyHaveNoLimits())
        {
            return true;
        }

        if (hexValue < propertyDefinition.Minimum || hexValue > propertyDefinition.Maximum)
        {
            observer.PropertyValueOutOfBound(Name, value);
            return false;
        }

        return true;
    }

    private bool ValidateFloatValue(string value, bool isNumeric, FloatCustomPropertyDefinition propertyDefinition, out float floatValue)
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

        if (DoesPropertyHaveNoLimits())
        {
            return true;
        }

        if (floatValue < propertyDefinition.Minimum || floatValue > propertyDefinition.Maximum)
        {
            observer.PropertyValueOutOfBound(Name, value);
            return false;
        }

        return true;
    }

    private bool ValidateStringValue(bool isNumeric)
    {
        if (isNumeric)
        {
            observer.PropertySyntaxError();
            return false;
        }

        return true;
    }

    private bool ValidateEnumValue(string value, bool isNumeric, EnumCustomPropertyDefinition propertyDefinition, out string? enumValue)
    {
        enumValue = null;
        if (isNumeric)
        {
            if (!TryGetEnumValueFromIndex(value, propertyDefinition, out enumValue))
            {
                return false;
            }
        }
        else
        {
            if (!propertyDefinition.Values.Contains(value))
            {
                observer.PropertyValueOutOfBound(Name, value);
                return false;
            }

            enumValue = value;
        }

        return true;
    }

    private bool TryGetEnumValueFromIndex(string value, EnumCustomPropertyDefinition propertyDefinition, out string? enumValue)
    {
        enumValue = null;
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
        {
            observer.PropertySyntaxError();
            return false;
        }

        if (index < 0 || index >= propertyDefinition.Values.Count)
        {
            observer.PropertyValueOutOfIndex(Name, value);
            return false;
        }

        enumValue = propertyDefinition.Values.ElementAt(index);
        return true;
    }

    private bool DoesPropertyHaveNoLimits()
    {
        switch (PropertyDefinition)
        {
            case IntegerCustomPropertyDefinition integerCustomPropertyDefinition:
                return integerCustomPropertyDefinition is { Minimum: 0, Maximum: 0 };
            case HexCustomPropertyDefinition hexCustomPropertyDefinition:
                return hexCustomPropertyDefinition is { Minimum: 0, Maximum: 0 };
            case FloatCustomPropertyDefinition floatCustomPropertyDefinition:
                return ExtensionsAndHelpers.AreDoublesEqual(floatCustomPropertyDefinition.Minimum, 0) && ExtensionsAndHelpers.AreDoublesEqual(floatCustomPropertyDefinition.Maximum, 0);
            default:
                return false;
        }
    }
}

public interface ICustomPropertyDefinition { } // Marker interface

public class FloatCustomPropertyDefinition : ICustomPropertyDefinition
{
    public double Maximum { get; }
    public double Minimum { get; }
    public double? Default { get; internal set; }
    
    public FloatCustomPropertyDefinition(double maximum, double minimum)
    {
        Maximum = maximum;
        Minimum = minimum;
    }
}

public class IntegerCustomPropertyDefinition : ICustomPropertyDefinition
{
    public int Maximum { get; }
    public int Minimum { get; }
    public int? Default { get; internal set; }
    
    public IntegerCustomPropertyDefinition(int maximum, int minimum)
    {
        Maximum = maximum;
        Minimum = minimum;
    }
}

public class HexCustomPropertyDefinition : ICustomPropertyDefinition
{
    public int Maximum { get; }
    public int Minimum { get; }
    public int? Default { get; internal set; }
    
    public HexCustomPropertyDefinition(int maximum, int minimum)
    {
        Maximum = maximum;
        Minimum = minimum;
    }
}

public class StringCustomPropertyDefinition : ICustomPropertyDefinition
{
    public string? Default { get; internal set; }
}

public class EnumCustomPropertyDefinition : ICustomPropertyDefinition
{
    public string? Default { get; internal set; }
    public IReadOnlyCollection<string> Values { get; }
    
    public EnumCustomPropertyDefinition(string[] values)
    {
        Values = values;
    }
}

public interface ICustomPropertyValue { } // Marker interface

public class PropertyValueUndefined : ICustomPropertyValue { } // Used in case no value was set and no default exists

public class FloatPropertyValue : ICustomPropertyValue
{
    public double Value { get; }
    
    public FloatPropertyValue(double value)
    {
        Value = value;
    }
}

public class IntegerPropertyValue : ICustomPropertyValue
{
    public int Value { get; }
    
    public IntegerPropertyValue(int value)
    {
        Value = value;
    }
}

public class HexPropertyValue : ICustomPropertyValue
{
    public int Value { get; }
    
    public HexPropertyValue(int value)
    {
        Value = value;
    }
}

public class StringPropertyValue : ICustomPropertyValue
{
    public string Value { get; }
    
    public StringPropertyValue(string value)
    {
        Value = value;
    }
}

public class EnumPropertyValue : ICustomPropertyValue
{
    public string Value { get; }
    
    public EnumPropertyValue(string value)
    {
        Value = value;
    }
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