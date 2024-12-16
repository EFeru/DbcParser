using DbcParserLib.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DbcParserLib.Generators
{
    public class DbcGenerator
    {
        public static bool MergeDbc(List<Dbc> dbcs, out Dbc dbcOutput)
        {
            dbcOutput = null;
            bool retVal = false;
            var allNodes = new List<Node>();
            var allMessages = new List<Message>();
            var allEnvironmentVariables = new List<EnvironmentVariable>();
            var allGlobalProperties = new List<CustomProperty>();
            foreach (Dbc dbc in dbcs)
            {

                foreach (var node in dbc.Nodes)
                {
                    if (!allNodes.Any(n => n.Name == node.Name))
                    {
                        allNodes.Add(node);
                    }
                }

                foreach (var message in dbc.Messages)
                {
                    if (!allMessages.Any(m => m.Name == message.Name))
                    {
                        allMessages.Add(message);
                    }
                }

                foreach (var envVar in dbc.EnvironmentVariables)
                {
                    if (!allEnvironmentVariables.Any(e => e.Name == envVar.Name))
                    {
                        allEnvironmentVariables.Add(envVar);
                    }
                }

                foreach (var globalProp in dbc.GlobalProperties)
                {

                    if (!allGlobalProperties.Any(g => g.CustomPropertyDefinition == globalProp.CustomPropertyDefinition))
                    {
                        allGlobalProperties.Add(globalProp);
                    }
                    else
                    {
                        var existingProperty = allGlobalProperties.First(g => g.CustomPropertyDefinition == globalProp.CustomPropertyDefinition);
                        // existingProperty.SetCustomPropertyValueFromDefault(); 
                    }
                }

                dbcOutput = new Dbc(allNodes, allMessages, allEnvironmentVariables, allGlobalProperties);
                if (dbcOutput?.Messages?.Count() > 0)
                {
                    retVal = true;
                }                 
            }
            return retVal;
        }
        public static void WriteToFile(Dbc dbc, string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                WriteToStream(dbc, fileStream);
            }
        }

        public static void WriteToStream(Dbc dbc, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {
                WriteToWriter(dbc, writer);
            }
        }

        public static void WriteToWriter(Dbc dbc, TextWriter writer)
        {
            WriteVersion(dbc, writer);
            WriteNewSymbols(dbc, writer);
            WriteBitTiming(dbc, writer);
            WriteNodes(dbc, writer);
            WriteMessages(dbc, writer);
            //WriteEnvironmentVariables(dbc, writer);
            WriteMessageComments(dbc, writer);
            WriteCustomProperties(dbc, writer);
            WriteCustomPropertyDefaultValues(dbc, writer);
            WriteCustomPropertyValues(dbc, writer);
            WriteValueTables(dbc, writer);
        }

        private static void WriteCustomProperties(Dbc dbc, TextWriter writer)
        {
            var addedProperties = new HashSet<string>();
            foreach (var message in dbc.Messages)
            {
                foreach (var customPropertyDefinition in message.CustomProperties.Values.Select(cp => cp.CustomPropertyDefinition).Distinct())
                {
                    if (addedProperties.Add(customPropertyDefinition.Name))
                    {
                        string objectType = customPropertyDefinition.DataType switch
                        {
                            CustomPropertyDataType.Integer => $"INT {customPropertyDefinition.IntegerCustomProperty.Minimum} {customPropertyDefinition.IntegerCustomProperty.Maximum}",
                            CustomPropertyDataType.Hex => "HEX",
                            CustomPropertyDataType.Float => "FLOAT",
                            CustomPropertyDataType.String => "STRING",
                            CustomPropertyDataType.Enum => $"ENUM {string.Join(",", customPropertyDefinition.EnumCustomProperty.Values.Select(v => $"\"{v}\""))}",
                            _ => "STRING"
                        };
                        writer.WriteLine($"BA_DEF_ BO_ \"{customPropertyDefinition.Name}\" {objectType};");
                    }
                }

                foreach (var signal in message.Signals)
                {
                    foreach (var customPropertyDefinition in signal.CustomProperties.Values.Select(cp => cp.CustomPropertyDefinition).Distinct())
                    {
                        if (addedProperties.Add(customPropertyDefinition.Name))
                        {
                            string objectType = customPropertyDefinition.DataType switch
                            {
                                CustomPropertyDataType.Integer => $"INT {customPropertyDefinition.IntegerCustomProperty.Minimum} {customPropertyDefinition.IntegerCustomProperty.Maximum}",
                                CustomPropertyDataType.Hex => "HEX",
                                CustomPropertyDataType.Float => "FLOAT",
                                CustomPropertyDataType.String => "STRING",
                                CustomPropertyDataType.Enum => $"ENUM {string.Join(",", customPropertyDefinition.EnumCustomProperty.Values.Select(v => $"\"{v}\""))}",
                                _ => "STRING"
                            };
                            writer.WriteLine($"BA_DEF_ SG_ \"{customPropertyDefinition.Name}\" {objectType};");
                        }
                    }
                }
            }
            foreach (var node in dbc.Nodes)
            {
                foreach (var customPropertyDefinition in node.CustomProperties.Values.Select(cp => cp.CustomPropertyDefinition).Distinct())
                {
                    if (addedProperties.Add(customPropertyDefinition.Name))
                    {
                        string objectType = customPropertyDefinition.DataType switch
                        {
                            CustomPropertyDataType.Integer => $"INT {customPropertyDefinition.IntegerCustomProperty.Minimum} {customPropertyDefinition.IntegerCustomProperty.Maximum}",
                            CustomPropertyDataType.Hex => "HEX",
                            CustomPropertyDataType.Float => "FLOAT",
                            CustomPropertyDataType.String => "STRING",
                            CustomPropertyDataType.Enum => $"ENUM {string.Join(",", customPropertyDefinition.EnumCustomProperty.Values.Select(v => $"\"{v}\""))}",
                            _ => "STRING"
                        };
                        writer.WriteLine($"BA_DEF_ BU_ \"{customPropertyDefinition.Name}\" {objectType};");
                    }
                }
            }
            foreach (var customPropertyDefinition in dbc.GlobalProperties.Select(gp => gp.CustomPropertyDefinition).Distinct())
            {
                if (addedProperties.Add(customPropertyDefinition.Name))
                {
                    string objectType = customPropertyDefinition.DataType switch
                    {
                        CustomPropertyDataType.Integer => $"INT {customPropertyDefinition.IntegerCustomProperty.Minimum} {customPropertyDefinition.IntegerCustomProperty.Maximum}",
                        CustomPropertyDataType.Hex => "HEX",
                        CustomPropertyDataType.Float => "FLOAT",
                        CustomPropertyDataType.String => "STRING",
                        CustomPropertyDataType.Enum => $"ENUM {string.Join(",", customPropertyDefinition.EnumCustomProperty.Values.Select(v => $"\"{v}\""))}",
                        _ => "STRING"
                    };
                    writer.WriteLine($"BA_DEF_ \"{customPropertyDefinition.Name}\" {objectType};");
                }
            }

            writer.WriteLine("");
        }

        private static void WriteCustomPropertyDefaultValues(Dbc dbc, TextWriter writer)
        {
            var addedProperties = new HashSet<string>();
            foreach (var message in dbc.Messages)
            {
                foreach (var customProperty in message.CustomProperties.Values)
                {
                    if (addedProperties.Add(customProperty.CustomPropertyDefinition.Name))
                    {
                        writer.WriteLine($"BA_DEF_DEF_ \"{customProperty.CustomPropertyDefinition.Name}\" {GetCustomPropertyDefaultValue(customProperty)};");
                    }
                }
                foreach (var signal in message.Signals)
                {
                    foreach (var customProperty in signal.CustomProperties.Values)
                    {
                        if (addedProperties.Add(customProperty.CustomPropertyDefinition.Name))
                        {
                            writer.WriteLine($"BA_DEF_DEF_ \"{customProperty.CustomPropertyDefinition.Name}\" {GetCustomPropertyDefaultValue(customProperty)};");
                        }
                    }
                }
            }
            foreach (var node in dbc.Nodes)
            {
                foreach (var customProperty in node.CustomProperties.Values)
                {
                    if (addedProperties.Add(customProperty.CustomPropertyDefinition.Name))
                    {
                        writer.WriteLine($"BA_DEF_DEF_ \"{customProperty.CustomPropertyDefinition.Name}\" {GetCustomPropertyDefaultValue(customProperty)};");
                    }
                }
            }
            foreach (var customProperty in dbc.GlobalProperties)
            {
                if (addedProperties.Add(customProperty.CustomPropertyDefinition.Name))
                {
                    writer.WriteLine($"BA_DEF_DEF_ \"{customProperty.CustomPropertyDefinition.Name}\" {GetCustomPropertyDefaultValue(customProperty)};");
                }
            }
            writer.WriteLine("");
        }



        private static void WriteVersion(Dbc dbc, TextWriter writer, string version = "")
        {
            writer.WriteLine($"VERSION \"{version}\"");
            writer.WriteLine("");
        }

        private static void WriteNewSymbols(Dbc dbc, TextWriter writer)
        {
            string[] NewSymbols =
            {
                "NS_DESC_",
                "CM_",
                "BA_DEF_",
                "BA_",
                "VAL_",
                "CAT_DEF_",
                "CAT_",
                "FILTER",
                "BA_DEF_DEF_",
                "EV_DATA_",
                "ENVVAR_DATA_",
                "SGTYPE_",
                "SGTYPE_VAL_",
                "BA_DEF_SGTYPE_",
                "BA_SGTYPE_",
                "SIG_TYPE_REF_",
                "VAL_TABLE_",
                "SIG_GROUP_",
                "SIG_VALTYPE_",
                "SIGTYPE_VALTYPE_",
                "BO_TX_BU_",
                "BA_DEF_REL_",
                "BA_REL_",
                "BA_DEF_DEF_REL_",
                "BU_SG_REL_",
                "BU_EV_REL_",
                "BU_BO_REL_",
                "SG_MUL_VAL_"
            };
            writer.WriteLine($"NS_ :");
            foreach (string symbol in NewSymbols)
            {
                writer.WriteLine($"{symbol}");
            }
            writer.WriteLine("");
        }

        private static void WriteBitTiming(Dbc dbc, TextWriter writer)
        {
            writer.WriteLine($"BS_ : ");
            writer.WriteLine("");
        }

        private static void WriteNodes(Dbc dbc, TextWriter writer)
        {
            writer.WriteLine($"BU_: {string.Join(" ", dbc.Nodes.Select(node => node.Name))}");
            writer.WriteLine("");
        }

        private static void WriteMessages(Dbc dbc, TextWriter writer)
        {
            foreach (var message in dbc.Messages)
            {
                uint messageId = message.IsExtID ? message.ID | 0x80000000 : message.ID;
                writer.WriteLine($"BO_ {messageId} {message.Name}: {message.DLC} {message.Transmitter}");
                foreach (var signal in message.Signals)
                {
                    string receivers = signal.Receiver.Length switch
                    {
                        0 => "Vector__XXX",
                        1 => signal.Receiver[0],
                        _ => string.Join(",", signal.Receiver)
                    };
                    string valueTypeSymbol = signal.ValueType switch
                    {
                        DbcValueType.Signed => "-",
                        DbcValueType.Unsigned => "+",
                        DbcValueType.IEEEFloat => "+",
                        DbcValueType.IEEEDouble => "+",
                        _ => "+"
                    };
                    var multiplexingInfo = signal.MultiplexingInfo();
                    string multiplexorIndicator = multiplexingInfo.Role switch
                    {
                        MultiplexingRole.Multiplexor => " M",
                        MultiplexingRole.Multiplexed => $" m{multiplexingInfo.Group}",
                        _ => ""
                    };
                    writer.WriteLine($" SG_ {signal.Name}{multiplexorIndicator} : {signal.StartBit}|{signal.Length}@{signal.ByteOrder}{valueTypeSymbol} ({signal.Factor},{signal.Offset}) [{signal.Minimum}|{signal.Maximum}] \"{signal.Unit}\" {receivers}");
                }
                writer.WriteLine("");
            }

        }

        private static void WriteEnvironmentVariables(Dbc dbc, TextWriter writer)
        {
            foreach (var envVar in dbc.EnvironmentVariables)
            {
                writer.WriteLine($"EV_ {envVar.Name} : {envVar.Type} [{GetMinimum(envVar)}|{GetMaximum(envVar)}] \"{envVar.Unit}\" {GetInitialValue(envVar)} {GetID(envVar)} {GetAccessType(envVar)} {GetAccessNodes(envVar)}");
                foreach (var customProperty in envVar.CustomProperties)
                {
                    writer.WriteLine($"BA_ \"{customProperty.Key}\" EV_ {envVar.Name} {customProperty.Value};");
                }
            }
            writer.WriteLine("");
        }
        private static void WriteMessageComments(Dbc dbc, TextWriter writer)
        {
            foreach (var message in dbc.Messages)
            {
                uint messageId = message.IsExtID ? message.ID | 0x80000000 : message.ID;
                if (!string.IsNullOrEmpty(message.Comment))
                {
                    writer.WriteLine($"CM_ BO_ {messageId} \"{message.Comment}\";");
                }
                foreach (var signal in message.Signals)
                {
                    if (!string.IsNullOrEmpty(signal.Comment))
                    {
                        writer.WriteLine($"CM_ SG_ {messageId} {signal.Name} \"{signal.Comment}\";");
                    }
                }
            }
            writer.WriteLine("");
        }
        private static void WriteCustomPropertyValues(Dbc dbc, TextWriter writer)
        {
            var addedProperties = new HashSet<CustomProperty>();

            foreach (var globalProperty in dbc.GlobalProperties)
            {
                if (TryGetCustomPropertyValueWithNumber(globalProperty, out object value))
                {
                    var line = $"BA_ \"{globalProperty.CustomPropertyDefinition.Name}\" {value};";
                    if (addedProperties.Add(globalProperty))
                    {
                        writer.WriteLine(line);
                    }
                }
            }

            foreach (var node in dbc.Nodes)
            {
                foreach (var customProperty in node.CustomProperties.Values)
                {
                    if (TryGetCustomPropertyValueWithNumber(customProperty, out object value))
                    {
                        var line = $"BA_ \"{customProperty.CustomPropertyDefinition.Name}\" BU_ {node.Name} {value};";
                        if (addedProperties.Add(customProperty))
                        {
                            writer.WriteLine(line);
                        }
                    }

                }
            }

            foreach (var message in dbc.Messages)
            {
                uint messageId = message.IsExtID ? message.ID | 0x80000000 : message.ID;
                foreach (var customProperty in message.CustomProperties.Values)
                {
                    if (TryGetCustomPropertyValueWithNumber(customProperty, out object value))
                    {
                        var line = $"BA_ \"{customProperty.CustomPropertyDefinition.Name}\" BO_ {messageId} {value};";
                        if (addedProperties.Add(customProperty))
                        {
                            writer.WriteLine(line);
                        }
                    }
                }

                foreach (var signal in message.Signals)
                {
                    foreach (var customProperty in signal.CustomProperties.Values)
                    {
                        if (TryGetCustomPropertyValueWithNumber(customProperty, out object value))
                        {
                            var line = $"BA_ \"{customProperty.CustomPropertyDefinition.Name}\" SG_ {messageId} {signal.Name} {value};";
                            if (addedProperties.Add(customProperty))
                            {
                                writer.WriteLine(line);
                            }
                        }
                    }
                }
            }

            writer.WriteLine("");
        }

        private static void WriteValueTables(Dbc dbc, TextWriter writer)
        {
            foreach (var message in dbc.Messages)
            {
                uint messageId = message.IsExtID ? message.ID | 0x80000000 : message.ID;
                foreach (var signal in message.Signals)
                {
                    if (signal.ValueTableMap != null && signal.ValueTableMap.Count > 0)
                    {
                        writer.Write($"VAL_ {messageId} {signal.Name} ");
                        foreach (var kvp in signal.ValueTableMap)
                        {
                            writer.Write($"{kvp.Key} \"{kvp.Value}\" ");
                        }
                        writer.WriteLine(";");
                    }
                }
            }
        }

        private static object GetID(EnvironmentVariable envVar)
        {
            return envVar.ID;
        }

        private static object GetAccessType(EnvironmentVariable envVar)
        {
            return envVar.Access;
        }

        private static object GetAccessNodes(EnvironmentVariable envVar)
        {
            return envVar.AccessNodes;
        }

        private static object GetMinimum(EnvironmentVariable envVar)
        {
            return envVar.Type switch
            {
                EnvDataType.Integer => envVar.IntegerEnvironmentVariable.Minimum,
                EnvDataType.Float => envVar.FloatEnvironmentVariable.Minimum,
                _ => null
            };
        }

        private static object GetMaximum(EnvironmentVariable envVar)
        {
            return envVar.Type switch
            {
                EnvDataType.Integer => envVar.IntegerEnvironmentVariable.Maximum,
                EnvDataType.Float => envVar.FloatEnvironmentVariable.Maximum,
                _ => null
            };
        }

        private static object GetInitialValue(EnvironmentVariable envVar)
        {
            return envVar.Type switch
            {
                EnvDataType.Integer => envVar.IntegerEnvironmentVariable.Default,
                EnvDataType.Float => envVar.FloatEnvironmentVariable.Default,
                _ => null
            };
        }
        private static bool TryGetCustomPropertyValueWithNumber(CustomProperty customProperty, out object value)
        {
            value = customProperty.CustomPropertyDefinition.DataType switch
            {
                CustomPropertyDataType.Integer => (object)customProperty.IntegerCustomProperty?.Value,
                CustomPropertyDataType.Hex => customProperty.HexCustomProperty?.Value,
                CustomPropertyDataType.Float => customProperty.FloatCustomProperty?.Value,
                CustomPropertyDataType.String => $"\"{customProperty.StringCustomProperty?.Value}\"",
                CustomPropertyDataType.Enum => (object)Array.IndexOf(customProperty.CustomPropertyDefinition.EnumCustomProperty.Values, customProperty.EnumCustomProperty?.Value),
                _ => null
            };

            var defaultValue = customProperty.CustomPropertyDefinition.DataType switch
            {
                CustomPropertyDataType.Integer => (object)customProperty.CustomPropertyDefinition.IntegerCustomProperty.Default,
                CustomPropertyDataType.Hex => customProperty.CustomPropertyDefinition.HexCustomProperty.Default,
                CustomPropertyDataType.Float => customProperty.CustomPropertyDefinition.FloatCustomProperty.Default,
                CustomPropertyDataType.String => $"\"{customProperty.CustomPropertyDefinition.StringCustomProperty.Default}\"",
                CustomPropertyDataType.Enum => (object)Array.IndexOf(customProperty.CustomPropertyDefinition.EnumCustomProperty.Values, customProperty.CustomPropertyDefinition.EnumCustomProperty.Default),
                _ => null
            };

            return !Equals(value, defaultValue);
        }
        private static object GetCustomPropertyValue(CustomProperty customProperty)
        {
            return customProperty.CustomPropertyDefinition.DataType switch
            {
                CustomPropertyDataType.Integer => customProperty.IntegerCustomProperty?.Value,
                CustomPropertyDataType.Hex => customProperty.HexCustomProperty?.Value,
                CustomPropertyDataType.Float => customProperty.FloatCustomProperty?.Value,
                CustomPropertyDataType.String => customProperty.StringCustomProperty?.Value,
                CustomPropertyDataType.Enum => customProperty.EnumCustomProperty?.Value,
                _ => null
            };
        }
        private static object GetCustomPropertyDefaultValue(CustomProperty customProperty)
        {
            return customProperty.CustomPropertyDefinition.DataType switch
            {
                CustomPropertyDataType.Integer => customProperty.CustomPropertyDefinition.IntegerCustomProperty.Default,
                CustomPropertyDataType.Hex => customProperty.CustomPropertyDefinition.HexCustomProperty.Default,
                CustomPropertyDataType.Float => customProperty.CustomPropertyDefinition.FloatCustomProperty.Default,
                CustomPropertyDataType.String => $"\"{customProperty.CustomPropertyDefinition.StringCustomProperty.Default}\"",
                CustomPropertyDataType.Enum => $"\"{customProperty.CustomPropertyDefinition.EnumCustomProperty.Default}\"",
                _ => null
            };
        }

    }
}
