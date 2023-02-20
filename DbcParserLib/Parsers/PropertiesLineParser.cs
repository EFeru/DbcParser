using DbcParserLib.Model;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DbcParserLib.Parsers
{
    public class PropertiesLineParser : ILineParser
    {
        private const string PropertiesLineStarter = "BA_ ";
        private const string PropertiesDefinitionLineStarter = "BA_DEF_ ";
        private const string PropertiesDefinitionDefaultLineStarter = "BA_DEF_DEF_ ";
        private const string PropertyParsingRegex = @"BA_\s+""([a-zA-Z_][\w]*)""(?:\s+(?:(BU_|EV_)\s+([a-zA-Z_][\w]*))|\s+(?:(BO_)\s+(\d+))|\s+(?:(SG_)\s+(\d+)\s+([a-zA-Z_][\w]*)))?\s+(?:(-?\d+|[0-9.]+)|""([^""]*)"");";
        private const string PropertyDefinitionParsingRegex = @"BA_DEF_\s+(?:(BU_|BO_|SG_|EV_)\s+)?""([a-zA-Z_][\w]*)""\s+(?:(?:(INT|HEX)\s+(-?\d+)\s+(-?\d+))|(?:(FLOAT)\s+([0-9.]+)\s+([0-9.]+))|(STRING)|(?:(ENUM)\s+""([^""]*)""))\s*;";
        private const string PropertyDefinitionDefaultParsingRegex = @"BA_DEF_DEF_\s+""([a-zA-Z_][\w]*)""\s+(?:(-?\d+|[0-9.]+)|""([^""]*)"")\s*;";

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var cleanLine = line.Trim(' ');

            if (cleanLine.StartsWith(PropertiesLineStarter) == false)
                return false;

            if (cleanLine.StartsWith(PropertiesDefinitionDefaultLineStarter))
            {
                var match = Regex.Match(cleanLine, PropertyDefinitionDefaultParsingRegex);
                if (match.Success)
                {
                    builder.AddCustomPropertyDefaultValue(match.Groups[1].Value, match.Groups[2].Value);
                }
                return true;
            }

            if (cleanLine.StartsWith(PropertiesDefinitionLineStarter))
            {
                var match = Regex.Match(cleanLine, PropertyDefinitionParsingRegex);
                if (match.Success)
                {
                    var customProperty = new CustomProperty
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
                    if (match.Groups[3].Value == "INT")
                    {
                        customProperty.IntegerCustomProperty = new IntegerCustomProperty
                        {
                            Minimum = int.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture),
                            Maximum = int.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture),
                        };
                    }
                    else if (match.Groups[3].Value == "HEX")
                    {
                        dataType = DbcDataType.Hex;
                        customProperty.HexCustomProperty = new HexCustomProperty
                        {
                            Minimum = int.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture),
                            Maximum = int.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture),
                        };
                    }
                    else if (match.Groups[6].Value == "FLOAT")
                    {
                        dataType = DbcDataType.Float;
                        customProperty.FloatCustomProperty = new FloatCustomProperty
                        {
                            Minimum = double.Parse(match.Groups[7].Value, CultureInfo.InvariantCulture),
                            Maximum = double.Parse(match.Groups[8].Value, CultureInfo.InvariantCulture),
                        };
                    }
                    else if(match.Groups[9].Value == "STRING")
                        dataType = DbcDataType.String;
                    else if (match.Groups[10].Value == "ENUM") 
                    { 
                        dataType = DbcDataType.Enum;
                        customProperty.EnumCustomProperty = new EnumCustomProperty
                        {
                            Default = match.Groups[11].Value.Split(','),
                        }; 
                    }
                    customProperty.DataType = dataType;
                    builder.AddCustomProperty(objectType, customProperty);
                }
                return true;
            }

            if (cleanLine.StartsWith(PropertiesLineStarter))
            {
                var match = Regex.Match(cleanLine, PropertyParsingRegex);
                if (match.Success)
                {
                    if (match.Groups[4].Value == "BU_")
                        builder.AddNodeCustomProperty(match.Groups[1].Value, match.Groups[5].Value, match.Groups[6].Value);
                    if (match.Groups[4].Value == "BO_")
                    {
                        builder.AddMessageCustomProperty(match.Groups[1].Value, uint.Parse(match.Groups[10].Value, CultureInfo.InvariantCulture), match.Groups[3].Value);
                        if (match.Groups[1].Value == "GenMsgCycleTime")
                            builder.AddMessageCycleTime(uint.Parse(match.Groups[10].Value, CultureInfo.InvariantCulture), int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture));
                    }
                    if (match.Groups[4].Value == "SG_")
                    {
                        builder.AddSignalCustomProperty(match.Groups[1].Value, uint.Parse(match.Groups[15].Value, CultureInfo.InvariantCulture), match.Groups[16].Value, match.Groups[17].Value);
                        if (match.Groups[1].Value == "GenSigStartValue")
                            builder.AddSignalInitialValue(uint.Parse(match.Groups[15].Value, CultureInfo.InvariantCulture), match.Groups[16].Value, double.Parse(match.Groups[17].Value, CultureInfo.InvariantCulture));
                    }
                }
                return true;
            }
            return false;
        }
    }
}