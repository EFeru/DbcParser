using DbcParserLib.Model;
using DbcParserLib.Observers;
using NPOI.DDF;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace DbcParserLib.Parsers
{
    public class ExcelParser : IExcelParser
    {
        private const string GenMsgSendType = "GenMsgSendType";
        private const string VFrameFormat = "VFrameFormat";
        private const string GenSigStartValue = "GenSigStartValue";
        private const string GenMsgStartDelayTime = "GenMsgStartDelayTime";
        private const string GenMsgDelayTime = "GenMsgDelayTime";
        private const string GenMsgCycleTime = "GenMsgCycleTime";
        private const string BusType = "BusType";
        private const string ProtocolType = "ProtocolType";
        private string[,] table;
        private int table_row_count = 0;
        private int table_column_count = 0;
        private IDictionary<string, ExcelColumnConfigModel> columnMapping = new Dictionary<string, ExcelColumnConfigModel>();
        private int _nodeStartIndex = 0;
        private DbcBuilder _dbcBuilder = new DbcBuilder(new SilentFailureObserver());
        private IEnumerable<Node> _nodes;
        private IEnumerable<Message> _messages;
        private IEnumerable<EnvironmentVariable> _environmentVariables;
        private IEnumerable<CustomProperty> globalProperties;
        private IParseFailureObserver m_observer;
        public ExcelParser()
        {
            GenDefaultDictionary();
        }
        public ExcelParser(IDictionary<DictionaryColumnKey, ExcelColumnConfigModel> excelTitleDictionary, int nodeStartColumnIndex)
        {
            columnMapping.Clear();
            foreach (var key in excelTitleDictionary.Keys)
            {
                if (excelTitleDictionary.TryGetValue(key, out ExcelColumnConfigModel model))
                {
                    AddColumn(key.ToString(), model.Header);
                    UpdateColumnConfig(key.ToString(), model.ColumnIndex);
                }
            }
            SetNodeStartIndex(nodeStartColumnIndex);
        }
        public ExcelParser(IDictionary<DictionaryColumnKey, ExcelColumnConfigModel> excelTitleDictionary, string nodeStartColumnName)
        {
            columnMapping.Clear();
            foreach (var key in excelTitleDictionary.Keys)
            {
                if (excelTitleDictionary.TryGetValue(key, out ExcelColumnConfigModel model))
                {
                    AddColumn(key.ToString(), model.Header);
                    UpdateColumnConfig(key.ToString(), model.ColumnIndex);
                }
            }
            SetNodeStartIndex(nodeStartColumnName);
        }
        public void GenDefaultDictionary()
        {
            columnMapping.Clear();
            AddColumn(nameof(DictionaryColumnKey.MessageName), "Message\r\nName");
            AddColumn(nameof(DictionaryColumnKey.FrameFormat), "Frame\r\nFormat");
            AddColumn(nameof(DictionaryColumnKey.ID), "Message\r\nID");
            AddColumn(nameof(DictionaryColumnKey.MessageSendType), "Message\r\nSend Type");
            AddColumn(nameof(DictionaryColumnKey.CycleTime), "Cycle\r\nTime");
            AddColumn(nameof(DictionaryColumnKey.DataLength), "Data\r\nLength");
            AddColumn(nameof(DictionaryColumnKey.SignalName), "Signal\r\nName");
            AddColumn(nameof(DictionaryColumnKey.Description), "Description");
            AddColumn(nameof(DictionaryColumnKey.ByteOrder), "Byte\r\nOrder");
            AddColumn(nameof(DictionaryColumnKey.StartBit), "Start\r\nBit");
            AddColumn(nameof(DictionaryColumnKey.BitLength), "Bit\r\nLength");
            AddColumn(nameof(DictionaryColumnKey.Sign), "Sign");
            AddColumn(nameof(DictionaryColumnKey.Factor), "Factor");
            AddColumn(nameof(DictionaryColumnKey.Offset), "Offset");
            AddColumn(nameof(DictionaryColumnKey.MinimumPhysical), "Minimum\r\nPhysical");
            AddColumn(nameof(DictionaryColumnKey.MaximumPhysical), "Maximum\r\nPhysical");
            AddColumn(nameof(DictionaryColumnKey.DefaultValue), "Default\r\nValue");
            AddColumn(nameof(DictionaryColumnKey.Unit), "Unit");
            AddColumn(nameof(DictionaryColumnKey.ValueTable), "Value\r\nTable");
            _nodeStartIndex = columnMapping.Count;
        }

        public void AddColumn(string columnKey, string Header = "", double columnWidth = 0, bool visible = true)
        {
            if (!columnMapping.ContainsKey(columnKey))
            {
                columnMapping.Add(columnKey, new ExcelColumnConfigModel()
                {
                    Header = string.IsNullOrEmpty(Header) ? columnKey : Header,
                    IsVisible = visible,
                    ColumnIndex = columnMapping.Count,
                    ColumnWidth = columnWidth
                });
            }
        }
        public UpdateColumnConfigState UpdateColumnConfig(DictionaryColumnKey columnKey, int? columnIndex = null, string header = null)
        {
            if (!columnMapping.ContainsKey(columnKey.ToString()))
            {
                return UpdateColumnConfigState.ColumnKeyNotExists;
            }
            if (!columnIndex.HasValue)
            {
                return UpdateColumnConfigState.ColumnIndexError;
            }
            if (string.IsNullOrEmpty(header))
            {
                return UpdateColumnConfigState.HeaderError;
            }
            var columnConfig = columnMapping[columnKey.ToString()];
            columnConfig.ColumnIndex = columnIndex.Value;
            columnConfig.Header = header;
            return UpdateColumnConfigState.Success;
        }
        public UpdateColumnConfigState UpdateColumnConfig(string columnKey, int? columnIndex = null, string header = null)
        {
            if (!columnMapping.ContainsKey(columnKey))
            {
                return UpdateColumnConfigState.ColumnKeyNotExists;
            }
            if (!columnIndex.HasValue)
            {
                return UpdateColumnConfigState.ColumnIndexError;
            }
            if (string.IsNullOrEmpty(header))
            {
                return UpdateColumnConfigState.HeaderError;
            }
            var columnConfig = columnMapping[columnKey];

            columnConfig.ColumnIndex = columnIndex.Value;
            columnConfig.Header = header;
            return UpdateColumnConfigState.Success;
        }
        public UpdateColumnConfigState UpdateColumnConfig(DictionaryColumnKey columnKey, int columnIndex)
        {
            if (!columnMapping.ContainsKey(columnKey.ToString()))
            {
                return UpdateColumnConfigState.ColumnKeyNotExists;
            }
            var columnConfig = columnMapping[columnKey.ToString()];
            columnConfig.ColumnIndex = columnIndex;
            return UpdateColumnConfigState.Success;
        }
        public UpdateColumnConfigState UpdateColumnConfig(string columnKey, int columnIndex)
        {
            if (!columnMapping.ContainsKey(columnKey))
            {
                return UpdateColumnConfigState.ColumnKeyNotExists;
            }
            var columnConfig = columnMapping[columnKey];
            columnConfig.ColumnIndex = columnIndex;
            return UpdateColumnConfigState.Success;
        }
        public UpdateColumnConfigState UpdateColumnConfig(DictionaryColumnKey columnKey, string header)
        {
            if (!columnMapping.ContainsKey(columnKey.ToString()))
            {
                return UpdateColumnConfigState.ColumnKeyNotExists;
            }
            if (string.IsNullOrEmpty(header))
            {
                return UpdateColumnConfigState.HeaderError;
            }
            var columnConfig = columnMapping[columnKey.ToString()];
            columnConfig.Header = header;
            return UpdateColumnConfigState.Success;
        }
        public UpdateColumnConfigState UpdateColumnConfig(string columnKey, string header)
        {
            if (!columnMapping.ContainsKey(columnKey))
            {
                return UpdateColumnConfigState.ColumnKeyNotExists;
            }
            if (string.IsNullOrEmpty(header))
            {
                return UpdateColumnConfigState.HeaderError;
            }
            var columnConfig = columnMapping[columnKey];
            columnConfig.Header = header;
            return UpdateColumnConfigState.Success;
        }
        private void AddNodeDictionary()
        {
            for (int i = _nodeStartIndex; i < table_column_count; i++)
            {
                AddColumn(table[0, i], table[0, i]);
            }
        }
        public ExcelParserState ParseFromPath(string path, out Dbc dbc)
        {
            dbc = null;
            string extension = Path.GetExtension(path);
            IWorkbook workbook;

            try
            {
                using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    if (extension.Equals(".xls"))
                    {
                        workbook = new HSSFWorkbook(file);
                    }
                    else if (extension.Equals(".xlsx"))
                    {
                        workbook = new XSSFWorkbook(file);
                    }
                    else
                    {
                        return ExcelParserState.FormatError;
                    }
                }

                // 读取Excel内容的逻辑
                ISheet sheet = workbook.GetSheetAt(0);
                table_row_count = sheet.LastRowNum + 1;
                table_column_count = sheet.GetRow(0).LastCellNum;
                table = new string[table_row_count, table_column_count];
                for (int row = 0; row <= sheet.LastRowNum; row++)
                {
                    IRow currentRow = sheet.GetRow(row);
                    if (currentRow != null)
                    {
                        for (int col = 0; col < table_column_count; col++)
                        {
                            ICell cell = currentRow.GetCell(col);
                            if (cell != null)
                            {
                                table[row, col] = cell.ToString();
                            }
                        }
                    }
                }
                AddNodeDictionary();
                //Paser to Dbc file
                ParseNodesFromTable();
                ParseMessageFromTable();
                dbc = _dbcBuilder.Build();
                // 假设解析成功，返回Success状态
                return ExcelParserState.Success;
            }
            catch (DirectoryNotFoundException)
            {
                return ExcelParserState.PathError;
            }
            catch (UnauthorizedAccessException)
            {
                return ExcelParserState.ReadPermissionError;
            }
            catch (IOException)
            {
                return ExcelParserState.ReadPermissionError;
            }
            catch (Exception ex)
            {
                return ExcelParserState.UnknownError;
            }
        }
        public ExcelParserState ParseFromPath(string path, string sheetName, out Dbc dbc)
        {
            dbc = null;
            string extension = Path.GetExtension(path);
            IWorkbook workbook;

            try
            {
                using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    if (extension.Equals(".xls"))
                    {
                        workbook = new HSSFWorkbook(file);
                    }
                    else if (extension.Equals(".xlsx"))
                    {
                        workbook = new XSSFWorkbook(file);
                    }
                    else
                    {
                        return ExcelParserState.FormatError;
                    }
                }

                // 读取Excel内容的逻辑
                ISheet sheet = workbook.GetSheet(sheetName);
                for (int row = 0; row <= sheet.LastRowNum; row++)
                {
                    IRow currentRow = sheet.GetRow(row);
                    if (currentRow != null)
                    {
                        for (int col = 0; col < currentRow.LastCellNum; col++)
                        {
                            ICell cell = currentRow.GetCell(col);
                            if (cell != null)
                            {
                                // 处理单元格内容
                                string cellValue = cell.ToString();
                                // 根据需要解析cellValue
                            }
                        }
                    }
                }

                // 假设解析成功，返回Success状态
                return ExcelParserState.Success;
            }
            catch (DirectoryNotFoundException)
            {
                return ExcelParserState.PathError;
            }
            catch (UnauthorizedAccessException)
            {
                return ExcelParserState.ReadPermissionError;
            }
            catch (IOException)
            {
                return ExcelParserState.ReadPermissionError;
            }
            catch (Exception)
            {
                return ExcelParserState.UnknownError;
            }
        }
        public ExcelParserState ParseFromPath(string path, int sheetIndex, out Dbc dbc)
        {
            dbc = null;
            string extension = Path.GetExtension(path);
            IWorkbook workbook;

            try
            {
                using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    if (extension.Equals(".xls"))
                    {
                        workbook = new HSSFWorkbook(file);
                    }
                    else if (extension.Equals(".xlsx"))
                    {
                        workbook = new XSSFWorkbook(file);
                    }
                    else
                    {
                        return ExcelParserState.FormatError;
                    }
                }

                // 读取Excel内容的逻辑
                ISheet sheet = workbook.GetSheetAt(sheetIndex);
                for (int row = 0; row <= sheet.LastRowNum; row++)
                {
                    IRow currentRow = sheet.GetRow(row);
                    if (currentRow != null)
                    {
                        for (int col = 0; col < currentRow.LastCellNum; col++)
                        {
                            ICell cell = currentRow.GetCell(col);
                            if (cell != null)
                            {
                                // 处理单元格内容
                                string cellValue = cell.ToString();
                                // 根据需要解析cellValue
                            }
                        }
                    }
                }

                // 假设解析成功，返回Success状态
                return ExcelParserState.Success;
            }
            catch (DirectoryNotFoundException)
            {
                return ExcelParserState.PathError;
            }
            catch (UnauthorizedAccessException)
            {
                return ExcelParserState.ReadPermissionError;
            }
            catch (IOException)
            {
                return ExcelParserState.ReadPermissionError;
            }
            catch (Exception)
            {
                return ExcelParserState.UnknownError;
            }
        }

        public void SetNodeStartIndex(int nodeStartIndex)
        {
            _nodeStartIndex = nodeStartIndex;
        }

        public void SetNodeStartIndex(string excelColoumnName)
        {
            int columnIndex = 0;
            excelColoumnName = excelColoumnName.ToUpper(); // 将列名转换为大写
            for (int i = 0; i < excelColoumnName.Length; i++)
            {
                columnIndex *= 26;
                columnIndex += (excelColoumnName[i] - 'A' + 1);
            }
            columnIndex--; // Convert to zero-based index
            _nodeStartIndex = columnIndex;
        }

        public int GetNodeStartIndex()
        {
            return _nodeStartIndex;
        }
        private void ParseNodesFromTable()
        {
            try
            {
                for (int col = _nodeStartIndex; col < table_column_count; col++)
                {
                    string nodeName = table[0, col];
                    if (!string.IsNullOrEmpty(nodeName))
                    {
                        var node = new Node
                        {
                            Name = nodeName,
                        };
                        _dbcBuilder.AddNode(node);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message.ToString());
            }
            return;
        }
        private void ParseMessageFromTable()
        {
            AddCustomProperty();
            for (int row = 1; row < table_row_count; row++)
            {
                if (isMessageHeaderLine(row))
                {
                    string transmitter = getMessageRowTransmitterName(row);
                    uint id = convertToMsgId(table[row, columnMapping[DictionaryColumnKey.ID.ToString()].ColumnIndex]);
                    bool isExtId = id > 0x7FF ? true : false;
                    string messageSendType = table[row, columnMapping[DictionaryColumnKey.MessageSendType.ToString()].ColumnIndex];
                    var message = new Message
                    {
                        Name = table[row, columnMapping[DictionaryColumnKey.MessageName.ToString()].ColumnIndex],
                        ID = id,
                        Transmitter = transmitter,
                        Comment = table[row, columnMapping[DictionaryColumnKey.Description.ToString()].ColumnIndex],
                        DLC = Convert.ToByte(table[row, columnMapping[DictionaryColumnKey.DataLength.ToString()].ColumnIndex]),
                        IsExtID = isExtId,
                    };
                    _dbcBuilder.AddMessage(message);
                    _dbcBuilder.AddMessageCustomProperty(GenMsgSendType, id, messageSendType, false);
                    _dbcBuilder.AddMessageCustomProperty(VFrameFormat, id, table[row, columnMapping[DictionaryColumnKey.FrameFormat.ToString()].ColumnIndex], false);
                    _dbcBuilder.AddMessageCustomProperty(GenMsgCycleTime, id, table[row, columnMapping[DictionaryColumnKey.CycleTime.ToString()].ColumnIndex], true);
                    _dbcBuilder.AddMessageCustomProperty(GenMsgStartDelayTime, id, "0", true);
                    _dbcBuilder.AddMessageCustomProperty(GenMsgDelayTime, id, "0", true);
                    _dbcBuilder.AddMessageCustomProperty(GenSigStartValue, id, "0", true);
                }
                else
                {
                    string name = table[row, columnMapping[DictionaryColumnKey.SignalName.ToString()].ColumnIndex];
                    string comment = table[row, columnMapping[DictionaryColumnKey.Description.ToString()].ColumnIndex];
                    byte byteOrder = string.Equals(table[row, columnMapping[DictionaryColumnKey.ByteOrder.ToString()].ColumnIndex], "Intel", StringComparison.OrdinalIgnoreCase) ? (byte)1 : (byte)0;
                    ushort startBit = Convert.ToUInt16(table[row, columnMapping[DictionaryColumnKey.StartBit.ToString()].ColumnIndex]);
                    ushort length = Convert.ToUInt16(table[row, columnMapping[DictionaryColumnKey.BitLength.ToString()].ColumnIndex]);
                    DbcValueType valueType = (DbcValueType)Enum.Parse(typeof(DbcValueType), table[row, columnMapping[DictionaryColumnKey.Sign.ToString()].ColumnIndex], true);
                    double factor = Convert.ToDouble(table[row, columnMapping[DictionaryColumnKey.Factor.ToString()].ColumnIndex]);
                    double offset = Convert.ToDouble(table[row, columnMapping[DictionaryColumnKey.Offset.ToString()].ColumnIndex]);
                    double minimumPhysical = Convert.ToDouble(table[row, columnMapping[DictionaryColumnKey.MinimumPhysical.ToString()].ColumnIndex]);
                    double maximumPhysical = Convert.ToDouble(table[row, columnMapping[DictionaryColumnKey.MaximumPhysical.ToString()].ColumnIndex]);
                    double initialValue = Convert.ToDouble(table[row, columnMapping[DictionaryColumnKey.DefaultValue.ToString()].ColumnIndex]);
                    string unit = table[row, columnMapping[DictionaryColumnKey.Unit.ToString()].ColumnIndex];
                    IReadOnlyDictionary<int, string> valueTableMap = ParseValueTableMap(table[row, columnMapping[DictionaryColumnKey.ValueTable.ToString()].ColumnIndex]);
                    string[] reveiver = getSignalReceiver(row);
                    _dbcBuilder.AddSignal(
                        new Signal()
                        {
                            Name = name,
                            Comment = comment,
                            ByteOrder = byteOrder,
                            StartBit = startBit,
                            Length = length,
                            ValueType = valueType,
                            Factor = factor,
                            Offset = offset,
                            Minimum = minimumPhysical,
                            Maximum = maximumPhysical,
                            Unit = unit,
                            ValueTableMap = valueTableMap,
                            Receiver = reveiver,
                        });
                }
            }
            return;
        }
        public IReadOnlyDictionary<int, string> ParseValueTableMap(string valueTableString)
        {
            var valueTableMap = new Dictionary<int, string>();

            if (string.IsNullOrWhiteSpace(valueTableString))
            {
                return valueTableMap;
            }

            var entries = valueTableString.Replace(",", ":").Replace("：", ":").Replace("，", ":").Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var entry in entries)
            {
                var parts = entry.Split(new[] { ':' }, 2);
                if (parts.Length == 2 && parts[0].StartsWith("0x") && int.TryParse(parts[0].Substring(2), System.Globalization.NumberStyles.HexNumber, null, out int key))
                {
                    valueTableMap[key] = parts[1];
                }
            }

            return valueTableMap;
        }
        private uint convertToMsgId(string id)
        {
            if (id.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return Convert.ToUInt32(id.Substring(2), 16);
            }
            else
            {
                return Convert.ToUInt32(id);
            }
        }
        private string[] getSignalReceiver(int row)
        {
            List<string> receivers = new List<string>();
            if (!isMessageHeaderLine(row))
            {
                for (int i = _nodeStartIndex; i < table_column_count; i++)
                {
                    if (string.Equals(table[row, i].Trim(), "R", StringComparison.OrdinalIgnoreCase))
                    {
                        if (columnMapping.TryGetValue(table[0, i], out ExcelColumnConfigModel model))
                        {
                            receivers.Add(model.Header);
                        }
                    }

                }
                return receivers.ToArray();
            }
            else
            {
                return null;
            }
        }
        private void AddCustomProperty()
        {
            _dbcBuilder.AddCustomProperty(CustomPropertyObjectType.Message,
                new CustomPropertyDefinition(m_observer)
                {
                    DataType = CustomPropertyDataType.Enum,
                    Name = GenMsgSendType,
                    EnumCustomProperty =
                    new EnumCustomPropertyDefinition
                    {
                        Default = "cyclic",
                        Values = new string[] { "cyclic", "reserved", "cyclicIfActive", "reserved", "reserved", "reserved", "reserved", "reserved", "noMsgSendType" },
                    }
                });
            _dbcBuilder.AddCustomProperty(CustomPropertyObjectType.Message,
                new CustomPropertyDefinition(m_observer)
                {
                    DataType = CustomPropertyDataType.Enum,
                    Name = VFrameFormat,
                    EnumCustomProperty =
                    new EnumCustomPropertyDefinition
                    {
                        Default = "StandardCAN",
                        Values = new string[] { "StandardCAN", "ExtendedCAN", "reserved", "J1939PG" },
                    }
                });
            _dbcBuilder.AddCustomProperty(CustomPropertyObjectType.Message,
                new CustomPropertyDefinition(m_observer)
                {
                    DataType = CustomPropertyDataType.Integer,
                    Name = GenSigStartValue,
                    IntegerCustomProperty =
                    new NumericCustomPropertyDefinition<int>
                    {
                        Default = 0,
                        Minimum = 0,
                        Maximum = 10000,
                    }
                });
            _dbcBuilder.AddCustomProperty(CustomPropertyObjectType.Message,
                new CustomPropertyDefinition(m_observer)
                {
                    DataType = CustomPropertyDataType.Integer,
                    Name = GenMsgStartDelayTime,
                    IntegerCustomProperty =
                    new NumericCustomPropertyDefinition<int>
                    {
                        Default = 0,
                        Minimum = 0,
                        Maximum = 100000,
                    }
                });
            _dbcBuilder.AddCustomProperty(CustomPropertyObjectType.Message,
                new CustomPropertyDefinition(m_observer)
                {
                    DataType = CustomPropertyDataType.Integer,
                    Name = GenMsgDelayTime,
                    IntegerCustomProperty =
                    new NumericCustomPropertyDefinition<int>
                    {
                        Default = 0,
                        Minimum = 0,
                        Maximum = 1000,
                    }
                });
            _dbcBuilder.AddCustomProperty(CustomPropertyObjectType.Message,
                new CustomPropertyDefinition(m_observer)
                {
                    DataType = CustomPropertyDataType.Integer,
                    Name = GenMsgCycleTime,
                    IntegerCustomProperty =
                    new NumericCustomPropertyDefinition<int>
                    {
                        Default = 0,
                        Minimum = 0,
                        Maximum = 3600000,
                    }
                });
            _dbcBuilder.AddCustomProperty(CustomPropertyObjectType.Global,
                new CustomPropertyDefinition(m_observer)
                {
                    DataType = CustomPropertyDataType.String,
                    Name = BusType,
                    StringCustomProperty = new StringCustomPropertyDefinition
                    {
                        Default = "CAN",
                    }
                });
            //_dbcBuilder.AddCustomProperty(CustomPropertyObjectType.Global,
            //    new CustomPropertyDefinition(m_observer)
            //    {
            //        DataType = CustomPropertyDataType.String,
            //        Name = ProtocolType,
            //        StringCustomProperty = new StringCustomPropertyDefinition
            //        {
            //            Default = "J1939",
            //        }
            //    });
        }
        private string getMessageRowTransmitterName(int row)
        {
            for (int col = _nodeStartIndex; col < table_column_count; col++)
            {
                if (!string.IsNullOrEmpty(table[row, col]) && string.Equals(table[row, col].Trim(), "S", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var key in columnMapping.Keys)
                    {
                        if (columnMapping[key].ColumnIndex == col)
                        {
                            return columnMapping[key].Header;
                        }
                    }
                }
            }
            return "Vector__XXX";
        }
        private bool isMessageHeaderLine(int row)
        {
            if (row < table_row_count)
            {
                //MessageName
                if (columnMapping.TryGetValue(DictionaryColumnKey.MessageName.ToString(), out ExcelColumnConfigModel MessageNameValue))
                {
                    if (string.IsNullOrEmpty(table[row, MessageNameValue.ColumnIndex]))
                    {
                        return false;
                    }
                }
                //ID
                if (columnMapping.TryGetValue(DictionaryColumnKey.ID.ToString(), out ExcelColumnConfigModel IDValue))
                {
                    if (string.IsNullOrEmpty(table[row, IDValue.ColumnIndex]))
                    {
                        return false;
                    }
                }
                //SignalName
                if (columnMapping.TryGetValue(DictionaryColumnKey.SignalName.ToString(), out ExcelColumnConfigModel SignalNameValue))
                {
                    if (!string.IsNullOrEmpty(table[row, SignalNameValue.ColumnIndex]))
                    {
                        return false;
                    }
                }
                //ByteOrder
                if (columnMapping.TryGetValue(DictionaryColumnKey.ByteOrder.ToString(), out ExcelColumnConfigModel ByteOrderValue))
                {
                    if (!string.IsNullOrEmpty(table[row, ByteOrderValue.ColumnIndex]))
                    {
                        return false;
                    }
                }
                //StartBit
                if (columnMapping.TryGetValue(DictionaryColumnKey.StartBit.ToString(), out ExcelColumnConfigModel StartBitValue))
                {
                    if (!string.IsNullOrEmpty(table[row, StartBitValue.ColumnIndex]))
                    {
                        return false;
                    }
                }
                //BitLength
                if (columnMapping.TryGetValue(DictionaryColumnKey.BitLength.ToString(), out ExcelColumnConfigModel BitLengthValue))
                {
                    if (!string.IsNullOrEmpty(table[row, BitLengthValue.ColumnIndex]))
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

    }
}
