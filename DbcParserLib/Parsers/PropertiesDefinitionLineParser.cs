using DbcParserLib.Model;
using DbcParserLib.Observers;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DbcParserLib.Parsers
{
    internal class PropertiesDefinitionLineParser : ILineParser
    {
        private const string PropertiesDefinitionLineStarter = "BA_DEF_ ";
        private const string PropertiesDefinitionDefaultLineStarter = "BA_DEF_DEF_ ";
        private const string PropertyDefinitionParsingRegex = @"BA_DEF_(?:\s+(BU_|BO_|SG_|EV_))?\s+""([a-zA-Z_][\w]*)""\s+(?:(?:(INT|HEX)\s+(-?\d+)\s+(-?\d+))|(?:(FLOAT)\s+([\d\+\-eE.]+)\s+([\d\+\-eE.]+))|(STRING)|(?:(ENUM)\s+((?:""[^""]*"",+)*(?:""[^""]*""))))\s*;";
        private const string PropertyDefinitionDefaultParsingRegex = @"BA_DEF_DEF_\s+""([a-zA-Z_][\w]*)""\s+(-?\d+|[\d\+\-eE.]+|""[^""]*"")\s*;";

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
                var match = Regex.Match(cleanLine, PropertyDefinitionDefaultParsingRegex);
                if (match.Success)
                    builder.AddCustomPropertyDefaultValue(match.Groups[1].Value, match.Groups[2].Value.Replace("\"", ""), !match.Groups[2].Value.StartsWith("\""));
                else
                    m_observer.PropertyDefaultSyntaxError();
                return true;
            }

            if (cleanLine.StartsWith(PropertiesDefinitionLineStarter))
            {
                var match = Regex.Match(cleanLine, PropertyDefinitionParsingRegex);
                if (match.Success)
                {
                    var customProperty = new CustomPropertyDefinition(m_observer)
                    {
                        Name = match.Groups[2].Value,
                    };

                    CustomPropertyObjectType objectType = CustomPropertyObjectType.Node;
                    switch (match.Groups[1].Value)
                    {
                        case "BO_":
                            objectType = CustomPropertyObjectType.Message; break;
                        case "SG_":
                            objectType = CustomPropertyObjectType.Signal; break;
                        case "EV_":
                            objectType = CustomPropertyObjectType.Environment; break;
                    }

                    CustomPropertyDataType dataType = CustomPropertyDataType.Integer;
                    if (match.Groups[3].Value == "INT")
                    {
                        customProperty.IntegerCustomProperty = new NumericCustomPropertyDefinition<int>
                        {
                            Minimum = int.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture),
                            Maximum = int.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture),
                        };
                    }
                    else if (match.Groups[3].Value == "HEX")
                    {
                        dataType = CustomPropertyDataType.Hex;
                        customProperty.HexCustomProperty = new NumericCustomPropertyDefinition<int>
                        {
                            Minimum = int.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture),
                            Maximum = int.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture),
                        };
                    }
                    else if (match.Groups[6].Value == "FLOAT")
                    {
                        dataType = CustomPropertyDataType.Float;
                        customProperty.FloatCustomProperty = new NumericCustomPropertyDefinition<double>
                        {
                            Minimum = double.Parse(match.Groups[7].Value, CultureInfo.InvariantCulture),
                            Maximum = double.Parse(match.Groups[8].Value, CultureInfo.InvariantCulture),
                        };
                    }
                    else if (match.Groups[9].Value == "STRING")
                    {
                        dataType = CustomPropertyDataType.String;
                        customProperty.StringCustomProperty = new StringCustomPropertyDefinition();
                    }
                    else if (match.Groups[10].Value.StartsWith("ENUM"))
                    {
                        dataType = CustomPropertyDataType.Enum;
                        customProperty.EnumCustomProperty = new EnumCustomPropertyDefinition
                        {
                            Values = match.Groups[11].Value.Replace("\"", "").Split(',')
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