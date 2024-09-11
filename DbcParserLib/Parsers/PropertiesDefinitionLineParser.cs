using DbcParserLib.Model;
using DbcParserLib.Observers;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DbcParserLib.Parsers
{
    internal class PropertiesDefinitionLineParser : ILineParser
    {
        private const string ObjectTypeGroup = "ObjectType";
        private const string AttributeNameGroup = "AttributeName";
        private const string AttributeValueGroup = "AttributeValue";
        private const string IsIntegerValueGroup = "IsIntegerValue";
        private const string MinIntGroup = "MinInt";
        private const string MaxIntGroup = "MaxInt";
        private const string MinFloatGroup = "MinFloat";
        private const string MaxFloatGroup = "MaxFloat";
        private const string IsFloatValueGroup = "IsFloatValue";
        private const string IsStringValueGroup = "IsStringValue";
        private const string IsEnumValueGroup = "IsEnumValue";
        private const string EnumValueGroup = "EnumValue";
        private const string PropertiesDefinitionLineStarter = "BA_DEF_ ";
        private const string PropertiesDefinitionDefaultLineStarter = "BA_DEF_DEF_ ";

        private readonly string m_propertyDefinitionParsingRegex = $@"BA_DEF_(?:\s+(?<{ObjectTypeGroup}>BU_|BO_|SG_|EV_))?\s+""(?<{AttributeNameGroup}>[a-zA-Z_][\w]*)""\s+" +
                                                                   $@"(?:(?:(?<{IsIntegerValueGroup}>INT|HEX)\s+(?<{MinIntGroup}>-?[\d\+eE]+)\s+(?<{MaxIntGroup}>-?[\d\+eE]+))|" +
                                                                   $@"(?:(?<{IsFloatValueGroup}>FLOAT)\s+(?<{MinFloatGroup}>[\d\+\-eE.]+)\s+(?<{MaxFloatGroup}>[\d\+\-eE.]+))" +
                                                                   $@"|(?<{IsStringValueGroup}>STRING)|(?:(?<{IsEnumValueGroup}>ENUM)\s+(?<{EnumValueGroup}>(?:""[^""]*"",\s*)*(?:""[^""]*""))))\s*;";

        private readonly string m_propertyDefinitionDefaultParsingRegex = $@"BA_DEF_DEF_\s+""(?<{AttributeNameGroup}>[a-zA-Z_][\w]*)""\s+(?<{AttributeValueGroup}>-?\d+|[\d\+\-eE.]+|""[^""]*"")\s*;";
        
        private readonly IParseFailureObserver m_observer;

        public PropertiesDefinitionLineParser(IParseFailureObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var cleanLine = line.Trim(' ');

            if (cleanLine.StartsWith(PropertiesDefinitionLineStarter) == false
                && cleanLine.StartsWith(PropertiesDefinitionDefaultLineStarter) == false)
                return false;

            if (cleanLine.StartsWith(PropertiesDefinitionDefaultLineStarter))
            {
                var match = Regex.Match(cleanLine, m_propertyDefinitionDefaultParsingRegex);
                if (match.Success)
                    builder.AddCustomPropertyDefaultValue(match.Groups[AttributeNameGroup].Value, match.Groups[AttributeValueGroup].Value.Replace(Helpers.DoubleQuotes, ""), !match.Groups[AttributeValueGroup].Value.StartsWith(Helpers.DoubleQuotes));
                else
                    m_observer.PropertyDefaultSyntaxError();
                return true;
            }

            if (cleanLine.StartsWith(PropertiesDefinitionLineStarter))
            {
                var match = Regex.Match(cleanLine, m_propertyDefinitionParsingRegex);
                if (match.Success)
                {
                    var customProperty = new CustomPropertyDefinition(m_observer)
                    {
                        Name = match.Groups[AttributeNameGroup].Value,
                    };

                    CustomPropertyObjectType objectType = CustomPropertyObjectType.Global;
                    switch (match.Groups[ObjectTypeGroup].Value)
                    {
                        case "BU_":
                            objectType = CustomPropertyObjectType.Node; break;
                        case "BO_":
                            objectType = CustomPropertyObjectType.Message; break;
                        case "SG_":
                            objectType = CustomPropertyObjectType.Signal; break;
                        case "EV_":
                            objectType = CustomPropertyObjectType.Environment; break;
                    }

                    CustomPropertyDataType dataType = CustomPropertyDataType.Integer;
                    if (match.Groups[IsIntegerValueGroup].Value.Equals("INT"))
                    {
                        //Since int.TryParse() is not able to parse scientic notation string we need to:
                        //Use double.TryParse() to parse digits or scientific notation string
                        //Cast values to integer (safe since regex accept only integer digits and positive exponent)
                        if(double.TryParse(match.Groups[MinIntGroup].Value, out var minValue) == false ||
                           double.TryParse(match.Groups[MaxIntGroup].Value, out var maxValue) == false)
                        {
                            m_observer.PropertyDefinitionSyntaxError();
                            return true;
                        }

                        customProperty.IntegerCustomProperty = new NumericCustomPropertyDefinition<int>
                        {
                            Minimum = Convert.ToInt32(minValue),
                            Maximum = Convert.ToInt32(maxValue)
                        };
                    }
                    else if (match.Groups[IsIntegerValueGroup].Value.Equals("HEX"))
                    {
                        dataType = CustomPropertyDataType.Hex;
                        customProperty.HexCustomProperty = new NumericCustomPropertyDefinition<int>
                        {
                            Minimum = int.Parse(match.Groups[MinIntGroup].Value, CultureInfo.InvariantCulture),
                            Maximum = int.Parse(match.Groups[MaxIntGroup].Value, CultureInfo.InvariantCulture),
                        };
                    }
                    else if (match.Groups[IsFloatValueGroup].Value.Equals("FLOAT"))
                    {
                        dataType = CustomPropertyDataType.Float;
                        customProperty.FloatCustomProperty = new NumericCustomPropertyDefinition<double>
                        {
                            Minimum = double.Parse(match.Groups[MinFloatGroup].Value, CultureInfo.InvariantCulture),
                            Maximum = double.Parse(match.Groups[MaxFloatGroup].Value, CultureInfo.InvariantCulture),
                        };
                    }
                    else if (match.Groups[IsStringValueGroup].Value.Equals("STRING"))
                    {
                        dataType = CustomPropertyDataType.String;
                        customProperty.StringCustomProperty = new StringCustomPropertyDefinition();
                    }
                    else if (match.Groups[IsEnumValueGroup].Value.StartsWith("ENUM"))
                    {
                        dataType = CustomPropertyDataType.Enum;
                        customProperty.EnumCustomProperty = new EnumCustomPropertyDefinition
                        {
                            Values = match.Groups[EnumValueGroup]
                                .Value
                                .Replace(Helpers.DoubleQuotes, string.Empty)
                                .Replace(Helpers.Space, string.Empty)
                                .Split(',')
                        };
                    }
                    customProperty.DataType = dataType;
                    builder.AddCustomProperty(objectType, customProperty);
                }
                else
                    m_observer.PropertyDefinitionSyntaxError();
                    
                return true;
            }

            return true;
        }
    }
}