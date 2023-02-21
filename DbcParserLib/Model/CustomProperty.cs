using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DbcParserLib.Model
{
    public class CustomProperty
    {
        private CustomPropertyDefinition m_customPropertyDefinition;

        public CustomProperty(CustomPropertyDefinition customPropertyDefinition)
        {
            m_customPropertyDefinition = customPropertyDefinition;
        }

        public IntegerCustomProperty IntegerCustomProperty { get; set; }
        public FloatCustomProperty FloatCustomProperty { get; set; }
        public StringCustomProperty StringCustomProperty { get; set; }

        public void SetCustomPropertyValue(string value)
        {
            switch (m_customPropertyDefinition.DataType)
            {
                case DbcDataType.Integer:
                    IntegerCustomProperty = new IntegerCustomProperty()
                    {
                        Value = int.Parse(value, CultureInfo.InvariantCulture)
                    };
                    break;
                case DbcDataType.Float:
                    FloatCustomProperty = new FloatCustomProperty()
                    {
                        Value = float.Parse(value, CultureInfo.InvariantCulture)
                    };
                    break;
                case DbcDataType.String:
                    StringCustomProperty = new StringCustomProperty()
                    {
                        Value = value
                    };
                    break;
                case DbcDataType.Enum:
                    StringCustomProperty = new StringCustomProperty()
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
                    IntegerCustomProperty = new IntegerCustomProperty()
                    {
                        Value = m_customPropertyDefinition.IntegerCustomProperty.Default
                    };
                    break;
                case DbcDataType.Float:
                    FloatCustomProperty = new FloatCustomProperty()
                    {
                        Value = m_customPropertyDefinition.FloatCustomProperty.Default
                    };
                    break;
                case DbcDataType.String:
                    StringCustomProperty = new StringCustomProperty()
                    {
                        Value = m_customPropertyDefinition.StringCustomProperty.Default
                    };
                    break;
                case DbcDataType.Enum:
                    StringCustomProperty = new StringCustomProperty()
                    {
                        Value = m_customPropertyDefinition.EnumCustomProperty.Default
                    };
                    break;
            }
        }
    }

    public class IntegerCustomProperty
    {
        public int Value { get; set; }
    }

    public class FloatCustomProperty
    {
        public double Value { get; set; }
    }
    public class StringCustomProperty
    {
        public string Value { get; set; }
    }
}
