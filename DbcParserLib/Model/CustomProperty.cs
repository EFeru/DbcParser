using System.Globalization;

namespace DbcParserLib.Model
{
    public class CustomProperty
    {
        public readonly CustomPropertyDefinition CustomPropertyDefinition;
        public CustomPropertyValue<int> IntegerCustomProperty { get; set; }
        public CustomPropertyValue<int> HexCustomProperty { get; set; }
        public CustomPropertyValue<double> FloatCustomProperty { get; set; }
        public CustomPropertyValue<string> StringCustomProperty { get; set; }
        public CustomPropertyValue<string> EnumCustomProperty { get; set; }

        public CustomProperty(CustomPropertyDefinition customPropertyDefinition)
        {
            CustomPropertyDefinition = customPropertyDefinition;
        }

        public void SetCustomPropertyValue(string value)
        {
            switch (CustomPropertyDefinition.DataType)
            {
                case CustomPropertyDataType.Integer:
                    IntegerCustomProperty = new CustomPropertyValue<int>()
                    {
                        Value = int.Parse(value.Replace("\"", ""), CultureInfo.InvariantCulture)
                    };
                    break;
                case CustomPropertyDataType.Hex:
                    HexCustomProperty = new CustomPropertyValue<int>()
                    {
                        Value = int.Parse(value.Replace("\"", ""), CultureInfo.InvariantCulture)
                    };
                    break;
                case CustomPropertyDataType.Float:
                    FloatCustomProperty = new CustomPropertyValue<double>()
                    {
                        Value = float.Parse(value.Replace("\"", ""), CultureInfo.InvariantCulture)
                    };
                    break;
                case CustomPropertyDataType.String:
                    StringCustomProperty = new CustomPropertyValue<string>()
                    {
                        Value = value.Replace("\"", "")
                    };
                    break;
                case CustomPropertyDataType.Enum:
                    EnumCustomProperty = new CustomPropertyValue<string>()
                    {
                        Value = CustomPropertyDefinition.TryGetEnumValue(value, out var enumValue) ? enumValue : value.Replace("\"", "")
                    };
                    break;
            }
        }

        public void SetCustomPropertyValueFromDefault()
        {
            switch (CustomPropertyDefinition.DataType)
            {
                case CustomPropertyDataType.Integer:
                    IntegerCustomProperty = new CustomPropertyValue<int>()
                    {
                        Value = CustomPropertyDefinition.IntegerCustomProperty.Default
                    };
                    break;
                case CustomPropertyDataType.Hex:
                    HexCustomProperty = new CustomPropertyValue<int>()
                    {
                        Value = CustomPropertyDefinition.HexCustomProperty.Default
                    };
                    break;
                case CustomPropertyDataType.Float:
                    FloatCustomProperty = new CustomPropertyValue<double>()
                    {
                        Value = CustomPropertyDefinition.FloatCustomProperty.Default
                    };
                    break;
                case CustomPropertyDataType.String:
                    StringCustomProperty = new CustomPropertyValue<string>()
                    {
                        Value = CustomPropertyDefinition.StringCustomProperty.Default
                    };
                    break;
                case CustomPropertyDataType.Enum:
                    EnumCustomProperty = new CustomPropertyValue<string>()
                    {
                        Value = CustomPropertyDefinition.EnumCustomProperty.Default
                    };
                    break;
            }
        }
    }

    public class CustomPropertyValue<T>
    {
        public T Value { get; set; }
    }
}
