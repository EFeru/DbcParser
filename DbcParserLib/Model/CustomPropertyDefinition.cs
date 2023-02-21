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
        public DbcDataType DataType { get; set; }
        public IntegerCustomPropertyDefinition IntegerCustomProperty { get; set; }
        public FloatCustomPropertyDefinition FloatCustomProperty { get; set; }
        public StringCustomPropertyDefinition StringCustomProperty { get; set; }
        public EnumCustomPropertyDefinition EnumCustomProperty { get; set; }

        public void SetCustomPropertyDefaultValue(string value)
        {
            switch (DataType)
            {
                case DbcDataType.Integer:
                    IntegerCustomProperty.Default = int.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case DbcDataType.Float:
                    FloatCustomProperty.Default = float.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case DbcDataType.String:
                    StringCustomProperty.Default = value;
                    break;
                case DbcDataType.Enum:
                    if (EnumCustomProperty.Definition.Contains(value))
                        EnumCustomProperty.Default = value;
                    break;
            }
        }
    }

    public class IntegerCustomPropertyDefinition
    {
        public int Maximum { get; set; }
        public int Minimum { get; set; }
        public int Default { get; set; }
    }

    public class FloatCustomPropertyDefinition
    {
        public double Maximum { get; set; }
        public double Minimum { get; set; }
        public double Default { get; set; }
    }
    public class StringCustomPropertyDefinition
    {
        public string Default { get; set; }
    }

    public class EnumCustomPropertyDefinition
    {
        public string Default { get; set; }
        public string[] Definition { get; set; }
    }

    public enum DbcObjectType
    {
        Node, Message, Signal, Environment
    }

    public enum DbcDataType
    {
        Integer, Float, String, Enum
    }
}
