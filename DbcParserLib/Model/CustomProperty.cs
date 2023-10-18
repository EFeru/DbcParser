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

        public bool SetCustomPropertyValue(string value, bool isNumeric)
        {
            switch (CustomPropertyDefinition.DataType)
            {
                case CustomPropertyDataType.Integer:
                    if(!CustomPropertyDefinition.TryGetIntegerValue(value, isNumeric, out var integerValue))
                        return false;

                    IntegerCustomProperty = new CustomPropertyValue<int>()
                    {
                        Value = integerValue
                    };
                    break;

                case CustomPropertyDataType.Hex:
                    if(!CustomPropertyDefinition.TryGetHexValue(value, isNumeric, out var hexValue))
                        return false;

                    HexCustomProperty = new CustomPropertyValue<int>()
                    {
                        Value = hexValue
                    };
                    break;

                case CustomPropertyDataType.Float:
                    if(!CustomPropertyDefinition.TryGetFloatValue(value, isNumeric, out var floatValue))
                        return false;
                    FloatCustomProperty = new CustomPropertyValue<double>()
                    {
                        Value = floatValue
                    };
                    break;

                case CustomPropertyDataType.String:
                    if(!CustomPropertyDefinition.IsString(isNumeric))
                        return false;
                    StringCustomProperty = new CustomPropertyValue<string>()
                    {
                        Value = value
                    };
                    break;

                case CustomPropertyDataType.Enum:
                    if(!CustomPropertyDefinition.TryGetEnumValue(value, isNumeric, out var enumValue))
                        return false;

                    EnumCustomProperty = new CustomPropertyValue<string>()
                    {
                        Value = enumValue
                    };
                    break;
            }
            return true;
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
