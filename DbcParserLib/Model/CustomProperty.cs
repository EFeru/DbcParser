using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DbcParserLib.Model
{
    public class CustomProperty
    {
        public readonly CustomPropertyDefinition m_customPropertyDefinition;

        public CustomProperty(CustomPropertyDefinition customPropertyDefinition)
        {
            m_customPropertyDefinition = customPropertyDefinition;
        }

        public CustomPropertyValue<int> IntegerCustomProperty { get; set; }
        public CustomPropertyValue<int> HexCustomProperty { get; set; }
        public CustomPropertyValue<double> FloatCustomProperty { get; set; }
        public CustomPropertyValue<string> StringCustomProperty { get; set; }
        public CustomPropertyValue<string> EnumCustomProperty { get; set; }

        public void SetCustomPropertyValue(string value)
        {
            switch (m_customPropertyDefinition.DataType)
            {
                case DbcDataType.Integer:
                    IntegerCustomProperty = new CustomPropertyValue<int>()
                    {
                        Value = int.Parse(value, CultureInfo.InvariantCulture)
                    };
                    break;
                case DbcDataType.Hex:
                    HexCustomProperty = new CustomPropertyValue<int>()
                    {
                        Value = int.Parse(value, CultureInfo.InvariantCulture)
                    };
                    break;
                case DbcDataType.Float:
                    FloatCustomProperty = new CustomPropertyValue<double>()
                    {
                        Value = float.Parse(value, CultureInfo.InvariantCulture)
                    };
                    break;
                case DbcDataType.String:
                    StringCustomProperty = new CustomPropertyValue<string>()
                    {
                        Value = value
                    };
                    break;
                case DbcDataType.Enum:
                    EnumCustomProperty = new CustomPropertyValue<string>()
                    {
                        Value = value
                    };
                    break;
            }
        }

        public void SetCustomPropertyValueFromDefault()
        {
            switch (m_customPropertyDefinition.DataType)
            {
                case DbcDataType.Integer:
                    IntegerCustomProperty = new CustomPropertyValue<int>()
                    {
                        Value = m_customPropertyDefinition.IntegerCustomProperty.Default
                    };
                    break;
                case DbcDataType.Hex:
                    HexCustomProperty = new CustomPropertyValue<int>()
                    {
                        Value = m_customPropertyDefinition.HexCustomProperty.Default
                    };
                    break;
                case DbcDataType.Float:
                    FloatCustomProperty = new CustomPropertyValue<double>()
                    {
                        Value = m_customPropertyDefinition.FloatCustomProperty.Default
                    };
                    break;
                case DbcDataType.String:
                    StringCustomProperty = new CustomPropertyValue<string>()
                    {
                        Value = m_customPropertyDefinition.StringCustomProperty.Default
                    };
                    break;
                case DbcDataType.Enum:
                    EnumCustomProperty = new CustomPropertyValue<string>()
                    {
                        Value = m_customPropertyDefinition.EnumCustomProperty.Default
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
