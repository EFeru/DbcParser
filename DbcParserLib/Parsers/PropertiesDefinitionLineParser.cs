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

            if (!cleanLine.StartsWith(PropertiesDefinitionLineStarter) && !cleanLine.StartsWith(PropertiesDefinitionDefaultLineStarter))
            {
                return false;
            }

            if (cleanLine.StartsWith(PropertiesDefinitionDefaultLineStarter))
            {
                var match = Regex.Match(cleanLine, PropertyDefinitionDefaultParsingRegex);
                if (match.Success)
                {
                    builder.AddCustomPropertyDefaultValue(match.Groups[1].Value, match.Groups[2].Value.Replace("\"", ""), !match.Groups[2].Value.StartsWith("\""));
                }
                else
                {
                    m_observer.PropertyDefaultSyntaxError();
                }

                return true;
            }

            if (cleanLine.StartsWith(PropertiesDefinitionLineStarter))
            {
                var match = Regex.Match(cleanLine, PropertyDefinitionParsingRegex);
                if (match.Success)
                {
                    CustomProperty? customPropertyDefinition = null;
                    var objectType = CustomPropertyObjectType.Node;
                    switch (match.Groups[1].Value)
                    {
                        case "BO_":
                            objectType = CustomPropertyObjectType.Message; break;
                        case "SG_":
                            objectType = CustomPropertyObjectType.Signal; break;
                        case "EV_":
                            objectType = CustomPropertyObjectType.Environment; break;
                    }
                    
                    if (match.Groups[3].Value == "INT")
                    {
                        var integerCustomProperty = new IntegerCustomPropertyDefinition
                        (
                            minimum : int.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture),
                            maximum : int.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture)
                        );
                        
                        customPropertyDefinition = new CustomProperty(match.Groups[2].Value, CustomPropertyDataType.Integer, integerCustomProperty, m_observer);
                    }
                    else if (match.Groups[3].Value == "HEX")
                    {
                        var hexCustomProperty = new HexCustomPropertyDefinition
                        (
                            minimum : int.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture),
                            maximum : int.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture)
                        );
                        
                        customPropertyDefinition = new CustomProperty(match.Groups[2].Value, CustomPropertyDataType.Hex, hexCustomProperty, m_observer);
                    }
                    else if (match.Groups[6].Value == "FLOAT")
                    {
                        var floatCustomProperty = new FloatCustomPropertyDefinition
                        (
                            minimum : double.Parse(match.Groups[7].Value, CultureInfo.InvariantCulture),
                            maximum : double.Parse(match.Groups[8].Value, CultureInfo.InvariantCulture)
                        );
                        
                        customPropertyDefinition = new CustomProperty(match.Groups[2].Value, CustomPropertyDataType.Float, floatCustomProperty, m_observer);
                    }
                    else if (match.Groups[9].Value == "STRING")
                    {
                        var stringCustomProperty = new StringCustomPropertyDefinition();
                        
                        customPropertyDefinition = new CustomProperty(match.Groups[2].Value, CustomPropertyDataType.String, stringCustomProperty, m_observer);
                    }
                    else if (match.Groups[10].Value.StartsWith("ENUM"))
                    {
                        var enumCustomProperty = new EnumCustomPropertyDefinition
                        (
                            values : match.Groups[11].Value.Replace("\"", "").Split(',')
                        );
                        
                        customPropertyDefinition = new CustomProperty(match.Groups[2].Value, CustomPropertyDataType.Enum, enumCustomProperty, m_observer);
                    }

                    if (customPropertyDefinition is not null)
                    {
                        builder.AddCustomProperty(objectType, customPropertyDefinition);
                        return true;
                    }
                    m_observer.PropertyDefinitionSyntaxError();
                }
                else
                {
                    m_observer.PropertyDefinitionSyntaxError();
                }

                return true;
            }

            return true;
        }
    }
}