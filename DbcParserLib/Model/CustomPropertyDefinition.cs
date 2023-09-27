using System.Globalization;
using System.Linq;
using DbcParserLib.Observers;

namespace DbcParserLib.Model
{
    public class CustomPropertyDefinition
    {
        private readonly IParseFailureObserver m_observer;
        public string Name { get; set; }
        public CustomPropertyDataType DataType { get; set; }
        public NumericCustomPropertyDefinition<int> IntegerCustomProperty { get; set; }
        public NumericCustomPropertyDefinition<int> HexCustomProperty { get; set; }
        public NumericCustomPropertyDefinition<double> FloatCustomProperty { get; set; }
        public StringCustomPropertyDefinition StringCustomProperty { get; set; }
        public EnumCustomPropertyDefinition EnumCustomProperty { get; set; }

        public CustomPropertyDefinition(IParseFailureObserver observer)
        {
            m_observer = observer;
        }


        public void SetCustomPropertyDefaultValue(string value, bool isNumeric)
        {
            switch (DataType)
            {
                case CustomPropertyDataType.Integer:
                    if(!TryGetIntegerValue(value, isNumeric, out var integerValue))
                        return;
                    IntegerCustomProperty.Default = integerValue;
                    break;

                case CustomPropertyDataType.Hex:
                    if(!TryGetHexValue(value, isNumeric, out var hexValue))
                        return;
                    HexCustomProperty.Default = hexValue;
                    break;

                case CustomPropertyDataType.Float:
                    if(!TryGetFloatValue(value, isNumeric, out var floatValue))
                        return;
                    FloatCustomProperty.Default = floatValue;
                    break;

                case CustomPropertyDataType.String:
                    if(!IsString(isNumeric))
                        return;
                    StringCustomProperty.Default = value;
                    break;

                case CustomPropertyDataType.Enum:
                    if(!TryGetEnumValue(value, isNumeric, out var enumValue))
                        return;
                    EnumCustomProperty.Default = enumValue;
                    break;
            }
        }

        internal bool TryGetIntegerValue(string value, bool isNumeric, out int integerValue)
        {
            integerValue = 0;
            if (!isNumeric)
            {
                m_observer.PropertySyntaxError();
                return false;
            }

            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out integerValue))
            {
                m_observer.PropertySyntaxError();
                return false;
            }

            if (integerValue < IntegerCustomProperty.Minimum || integerValue > IntegerCustomProperty.Maximum)
            {
                m_observer.PropertyValueOutOfBound(Name, value);
                return false;
            }

            return true;
        }

        internal bool TryGetHexValue(string value, bool isNumeric, out int hexValue)
        {
            hexValue = 0;
            if (!isNumeric)
            {
                m_observer.PropertySyntaxError();
                return false;
            }

            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out hexValue))
            {
                m_observer.PropertySyntaxError();
                return false;
            }

            if (hexValue < HexCustomProperty.Minimum || hexValue > HexCustomProperty.Maximum)
            {
                m_observer.PropertyValueOutOfBound(Name, value);
                return false;
            }

            return true;
        }

        internal bool TryGetFloatValue(string value, bool isNumeric, out float floatValue)
        {
            floatValue = 0;
            if (!isNumeric)
            {
                m_observer.PropertySyntaxError();
                return false;
            }

            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture,
                    out floatValue))
            {
                m_observer.PropertySyntaxError();
                return false;
            }

            if (floatValue < FloatCustomProperty.Minimum || floatValue > FloatCustomProperty.Maximum)
            {
                m_observer.PropertyValueOutOfBound(Name, value);
                return false;
            }

            return true;
        }

        internal bool IsString(bool isNumeric)
        {
            if (isNumeric)
            {
                m_observer.PropertySyntaxError();
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
                    m_observer.PropertyValueOutOfBound(Name, value);
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
                m_observer.PropertySyntaxError();
                return false;
            }

            if (index < 0 || index >= EnumCustomProperty.Values.Length)
            {
                m_observer.PropertyValueOutOfIndex(Name, value);
                return false;
            }
            
            enumValue = EnumCustomProperty.Values[index];
            return true;
        }
    }

    public class NumericCustomPropertyDefinition<T>
    {
        public T Maximum { get; set; }
        public T Minimum { get; set; }
        public T Default { get; set; }
    }

    public class StringCustomPropertyDefinition
    {
        public string Default { get; set; }
    }

    public class EnumCustomPropertyDefinition
    {
        public string Default { get; set; }
        public string[] Values { get; set; }
    }

    public enum CustomPropertyObjectType
    {
        Node, Message, Signal, Environment
    }

    public enum CustomPropertyDataType
    {
        Integer, Hex, Float, String, Enum
    }
}
