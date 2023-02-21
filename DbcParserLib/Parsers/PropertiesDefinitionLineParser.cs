using DbcParserLib.Model;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DbcParserLib.Parsers
{
    public class PropertiesDefinitionLineParser : ILineParser
    {
        private const string PropertiesDefinitionLineStarter = "BA_DEF_ ";
        private const string PropertiesDefinitionDefaultLineStarter = "BA_DEF_DEF_ ";
        private const string PropertyDefinitionParsingRegex = @"BA_DEF_\s+(?:(BU_|BO_|SG_|EV_)\s+)?""([a-zA-Z_][\w]*)""\s+(?:(?:(INT|HEX)\s+(-?\d+)\s+(-?\d+))|(?:(FLOAT)\s+([0-9.]+)\s+([0-9.]+))|(STRING)|(ENUM\s+(?:""[^""]*"",*){1,100}))\s*;";
        private const string PropertyDefinitionDefaultParsingRegex = @"BA_DEF_DEF_\s+""([a-zA-Z_][\w]*)""\s+(-?\d+|[0-9.]+|""[^""]*"")\s*;";

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
                {
                    builder.AddCustomPropertyDefaultValue(match.Groups[1].Value, match.Groups[2].Value.Replace("\"", ""));
                }
                return true;
            }

            if (cleanLine.StartsWith(PropertiesDefinitionLineStarter))
            {
                var match = Regex.Match(cleanLine, PropertyDefinitionParsingRegex);
                if (match.Success)
                {
                    var customProperty = new CustomPropertyDefinition
                    {
                        Name = match.Groups[2].Value,
                    };

                    DbcObjectType objectType = DbcObjectType.Node;
                    switch (match.Groups[1].Value)
                    {
                        case "BO_":
                            objectType = DbcObjectType.Message; break;
                        case "SG_":
                            objectType = DbcObjectType.Signal; break;
                        case "EV_":
                            objectType = DbcObjectType.Environment; break;
                    }

                    DbcDataType dataType = DbcDataType.Integer;
                    if (match.Groups[3].Value == "INT" || match.Groups[3].Value == "HEX")
                    {
                        customProperty.IntegerCustomProperty = new IntegerCustomPropertyDefinition
                        {
                            Minimum = int.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture),
                            Maximum = int.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture),
                        };
                    }
                    else if (match.Groups[6].Value == "FLOAT")
                    {
                        dataType = DbcDataType.Float;
                        customProperty.FloatCustomProperty = new FloatCustomPropertyDefinition
                        {
                            Minimum = double.Parse(match.Groups[7].Value, CultureInfo.InvariantCulture),
                            Maximum = double.Parse(match.Groups[8].Value, CultureInfo.InvariantCulture),
                        };
                    }
                    else if (match.Groups[9].Value == "STRING")
                        dataType = DbcDataType.String;
                    else if (match.Groups[10].Value.StartsWith("ENUM "))
                    {
                        dataType = DbcDataType.Enum;
                        var enumDefinition = match.Groups[10].Value.Replace("\"", "").Split(' ')[1];
                        customProperty.EnumCustomProperty = new EnumCustomPropertyDefinition
                        {
                            Definition = enumDefinition.Split(','),
                        };
                    }
                    customProperty.DataType = dataType;
                    builder.AddCustomProperty(objectType, customProperty);
                }
                return true;
            }
            return false;
        }
    }
}