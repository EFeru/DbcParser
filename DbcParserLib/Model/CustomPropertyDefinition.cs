using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

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
                    IntegerCustomProperty.Default = int.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case CustomPropertyDataType.Hex:
                    HexCustomProperty.Default = int.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case CustomPropertyDataType.Float:
                    FloatCustomProperty.Default = float.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case CustomPropertyDataType.String:
                    StringCustomProperty.Default = value;
                    break;
                case CustomPropertyDataType.Enum:
                    EnumCustomProperty.Default = value;
                    break;
            }
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
