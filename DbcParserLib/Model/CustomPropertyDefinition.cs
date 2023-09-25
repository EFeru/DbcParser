using System.Globalization;

namespace DbcParserLib.Model
{
    public class CustomPropertyDefinition
    {
        public string Name { get; set; }
        public CustomPropertyDataType DataType { get; set; }
        public NumericCustomPropertyDefinition<int> IntegerCustomProperty { get; set; }
        public NumericCustomPropertyDefinition<int> HexCustomProperty { get; set; }
        public NumericCustomPropertyDefinition<double> FloatCustomProperty { get; set; }
        public StringCustomPropertyDefinition StringCustomProperty { get; set; }
        public EnumCustomPropertyDefinition EnumCustomProperty { get; set; }

        public void SetCustomPropertyDefaultValue(string value)
        {
            switch (DataType)
            {
                case CustomPropertyDataType.Integer:
                    IntegerCustomProperty.Default = int.Parse(value.Replace("\"", ""), CultureInfo.InvariantCulture);
                    break;
                case CustomPropertyDataType.Hex:
                    HexCustomProperty.Default = int.Parse(value.Replace("\"", ""), CultureInfo.InvariantCulture);
                    break;
                case CustomPropertyDataType.Float:
                    FloatCustomProperty.Default = float.Parse(value.Replace("\"", ""), CultureInfo.InvariantCulture);
                    break;
                case CustomPropertyDataType.String:
                    StringCustomProperty.Default = value.Replace("\"", "");
                    break;
                case CustomPropertyDataType.Enum:
                    EnumCustomProperty.Default = TryGetEnumValue(value, out var enumValue) ? enumValue : value.Replace("\"", "");
                    break;
            }
        }

        internal bool TryGetEnumValue(string value, out string enumValue)
        {
            enumValue = null;

            if (value.Contains("\""))
                return false;

            if (EnumCustomProperty == null)
                return false;

            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
                return false;

            if (index < 0 || index >= EnumCustomProperty.Values.Length)
                return false;

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
