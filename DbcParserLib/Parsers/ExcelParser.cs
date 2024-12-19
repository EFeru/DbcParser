using DbcParserLib.Model;
using DbcParserLib.Observers;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Text.RegularExpressions;

namespace DbcParserLib.Parsers
{
    public class ExcelParser : IExcelParser
    {
        // Normal protocol
        private const string GenMsgSendType = "GenMsgSendType";
        private const string VFrameFormat = "VFrameFormat";
        private const string GenSigStartValue = "GenSigStartValue";
        private const string GenMsgStartDelayTime = "GenMsgStartDelayTime";
        private const string GenMsgDelayTime = "GenMsgDelayTime";
        private const string GenMsgCycleTime = "GenMsgCycleTime";
        private const string BusType = "BusType";
        //Used for j1939
        private const string ProtocolType = "ProtocolType";
        private const string NmStationAddress = "NmStationAddress";
        //1939 - 81 CA
        private const string NmJ1939IndustryGroup = "NmJ1939IndustryGroup";
        private const string NmJ1939System = "NmJ1939System";
        private const string NmJ1939SystemInstance = "NmJ1939SystemInstance";
        private const string NmJ1939Function = "NmJ1939Function";
        private const string NmJ1939FunctionInstance = "NmJ1939FunctionInstance";
        private const string NmJ1939ECUInstance = "NmJ1939ECUInstance";
        private const string NmJ1939ManufacturerCode = "NmJ1939ManufacturerCode";
        private const string NmJ1939IdentityNumber = "NmJ1939IdentityNumber";

        private string[,] table;
        private int table_row_count = 0;
        private int table_column_count = 0;
        private IDictionary<string, ExcelColumnConfigModel> columnMapping = new Dictionary<string, ExcelColumnConfigModel>();
        private int _nodeStartIndex = 0;
        private DbcBuilder _dbcBuilder = new DbcBuilder(new SilentFailureObserver());
        private IParseFailureObserver m_observer;
        private DbcProtocolType _protocolType = DbcProtocolType.CAN;
        private int _messageRowStartOffset = 1;
        private int _nodeRowIndex = 0;
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
                    UpdateColumnConfigWithIndex(key.ToString(), model.ColumnIndex);
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
                    UpdateColumnConfigWithIndex(key.ToString(), model.ColumnIndex);
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
            AddColumn(nameof(DictionaryColumnKey.DataType), "Sign");
            AddColumn(nameof(DictionaryColumnKey.Factor), "Factor");
            AddColumn(nameof(DictionaryColumnKey.Offset), "Offset");
            AddColumn(nameof(DictionaryColumnKey.MinimumPhysical), "Minimum\r\nPhysical");
            AddColumn(nameof(DictionaryColumnKey.MaximumPhysical), "Maximum\r\nPhysical");
            AddColumn(nameof(DictionaryColumnKey.InitialValue), "Default\r\nValue");
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
        public UpdateColumnConfigState UpdateColumnConfigWithIndex(DictionaryColumnKey columnKey, int columnIndex)
        {
            if (!columnMapping.ContainsKey(columnKey.ToString()))
            {
                return UpdateColumnConfigState.ColumnKeyNotExists;
            }
            var columnConfig = columnMapping[columnKey.ToString()];
            columnConfig.ColumnIndex = columnIndex;
            return UpdateColumnConfigState.Success;
        }
        public UpdateColumnConfigState UpdateColumnConfigWithIndex(string columnKey, int columnIndex)
        {
            if (!columnMapping.ContainsKey(columnKey))
            {
                return UpdateColumnConfigState.ColumnKeyNotExists;
            }
            var columnConfig = columnMapping[columnKey];
            columnConfig.ColumnIndex = columnIndex;
            return UpdateColumnConfigState.Success;
        }
        public UpdateColumnConfigState UpdateColumnConfigWithName(DictionaryColumnKey columnKey, string columnIndexName)
        {
            if (!columnMapping.ContainsKey(columnKey.ToString()))
            {
                return UpdateColumnConfigState.ColumnKeyNotExists;
            }
            var columnConfig = columnMapping[columnKey.ToString()];
            columnConfig.ColumnIndex = convertColumnIndexWithColumnName(columnIndexName);
            return UpdateColumnConfigState.Success;
        }

        public UpdateColumnConfigState UpdateColumnConfigWithName(string columnKey, string columnIndexName)
        {
            throw new NotImplementedException();
        }

        private void AddNodeDictionary()
        {
            for (int i = _nodeStartIndex; i < table_column_count; i++)
            {
                if (!string.IsNullOrEmpty(table[_nodeRowIndex, i]))
                {
                    AddColumn(table[_nodeRowIndex, i], table[_nodeRowIndex, i]);
                }
            }
        }
        public ExcelParserState ParseFirstSheetFromPath(string path, out Dbc dbc)
        {
            dbc = null;
            string extension = Path.GetExtension(path);
            IWorkbook workbook;
            ExcelParserState result = ExcelParserState.Success;
            try
            {
                createWorkbookFromPath(path, out workbook);
                // 读取Excel内容的逻辑
                ISheet sheet = workbook.GetSheetAt(0);
                if (sheet == null)
                {
                    return ExcelParserState.ReadNullError;
                }
                if ((result = createWorkingTable(sheet, out table)) != ExcelParserState.Success)
                {
                    return result;
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
        public ExcelParserState ParseSheetNameFromPath(string path, string sheetName, out Dbc dbc)
        {
            dbc = null;
            string extension = Path.GetExtension(path);
            IWorkbook workbook;
            ExcelParserState result = ExcelParserState.Success;
            try
            {
                createWorkbookFromPath(path, out workbook);
                // 读取Excel内容的逻辑
                ISheet sheet = workbook.GetSheet(sheetName);
                if (sheet == null)
                {
                    return ExcelParserState.ReadNullError;
                }
                if ((result = createWorkingTable(sheet, out table)) != ExcelParserState.Success)
                {
                    return result;
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
            catch (Exception)
            {
                return ExcelParserState.UnknownError;
            }
        }
        public ExcelParserState ParseSheetIndexFromPath(string path, int sheetIndex, out Dbc dbc)
        {
            dbc = null;
            string extension = Path.GetExtension(path);
            IWorkbook workbook;
            ExcelParserState result = ExcelParserState.Success;
            try
            {
                createWorkbookFromPath(path, out workbook);
                // 读取Excel内容的逻辑
                ISheet sheet = workbook.GetSheetAt(sheetIndex);
                if (sheet == null)
                {
                    return ExcelParserState.ReadNullError;
                }
                if ((result = createWorkingTable(sheet, out table)) != ExcelParserState.Success)
                {
                    return result;
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
            catch (Exception)
            {
                return ExcelParserState.UnknownError;
            }
        }
        private ExcelParserState createWorkbookFromPath(string path, out IWorkbook workbook)
        {
            workbook = null;
            string extension = Path.GetExtension(path);
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
            return ExcelParserState.UnknownError;
        }
        private ExcelParserState createWorkingTable(ISheet sheet, out string[,] table)
        {
            table = null;
            if (sheet == null)
            {
                return ExcelParserState.ReadNullError;
            }
            initSheetTable(sheet);
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
            return ExcelParserState.Success;
        }
        private void initSheetTable(ISheet sheet)
        {
            table_row_count = sheet.LastRowNum + 1;
            table_column_count = sheet.GetRow(0).LastCellNum > sheet.GetRow(_nodeRowIndex).LastCellNum ? sheet.GetRow(0).LastCellNum : sheet.GetRow(_nodeRowIndex).LastCellNum;
            table = new string[table_row_count, table_column_count];
        }
        public void SetNodeStartIndex(int nodeStartIndex)
        {
            _nodeStartIndex = nodeStartIndex;
        }

        public void SetNodeStartIndex(string excelColoumnName)
        {
            _nodeStartIndex = convertColumnIndexWithColumnName(excelColoumnName);
        }
        private int convertColumnIndexWithColumnName(string columnName)
        {
            int retVal = 0;
            columnName = Regex.Replace(columnName.ToUpper().Trim(), "[^A-Z]", "");
            for (int i = 0; i < columnName.Length; i++)
            {
                retVal *= 26;
                retVal += (columnName[i] - 'A' + 1);
            }
            retVal--; // Convert to zero-based index
            return retVal;
        }
        private string convertIndexToColumnName(int index)
        {
            if (index < 0)
                return string.Empty;

            string columnName = string.Empty;

            while (index >= 0)
            {
                int remainder = index % 26;
                columnName = (char)('A' + remainder) + columnName;
                index = index / 26 - 1;
            }
            return columnName;
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
            bool messageParsingResult = false;
            bool signalParsingResult = false;
            AddCustomProperty();
            for (int row = _messageRowStartOffset; row < table_row_count; row++)
            {
                if (isMessageHeaderLine(row))
                {
                    messageParsingResult |= parsing_MessageTransmitter(row, out string transmitter);
                    messageParsingResult |= parsing_MessageId(table[row, columnMapping[DictionaryColumnKey.ID.ToString()].ColumnIndex], out uint id);
                    bool isExtId = id > 0x7FF ? true : false;
                    messageParsingResult |= parsing_MsgSendType(table[row, columnMapping[DictionaryColumnKey.MessageSendType.ToString()].ColumnIndex], out string messageSendType);
                    messageParsingResult |= parsing_FrameFormat(table[row, columnMapping[DictionaryColumnKey.FrameFormat.ToString()].ColumnIndex], out string parsedFrameFormat);
                    messageParsingResult |= parsing_MessageCycleTime(table[row, columnMapping[DictionaryColumnKey.CycleTime.ToString()].ColumnIndex], out uint messageCycleTime);
                    var message = new Message
                    {
                        Name = table[row, columnMapping[DictionaryColumnKey.MessageName.ToString()].ColumnIndex],
                        ID = id,
                        Transmitter = transmitter,
                        Comment = table[row, columnMapping[DictionaryColumnKey.Description.ToString()].ColumnIndex],
                        DLC = Convert.ToByte(table[row, columnMapping[DictionaryColumnKey.DataLength.ToString()].ColumnIndex]),
                        IsExtID = isExtId,
                    };
                    if (messageParsingResult)
                    {
                        _dbcBuilder.AddMessage(message);
                        _dbcBuilder.AddMessageCustomProperty(GenMsgSendType, id, messageSendType, false);
                        _dbcBuilder.AddMessageCustomProperty(VFrameFormat, id, parsedFrameFormat, false);
                        _dbcBuilder.AddMessageCustomProperty(GenMsgCycleTime, id, messageCycleTime.ToString(), true);
                        _dbcBuilder.AddMessageCustomProperty(GenMsgStartDelayTime, id, "0", true);
                        _dbcBuilder.AddMessageCustomProperty(GenMsgDelayTime, id, "0", true);
                        _dbcBuilder.AddMessageCustomProperty(GenSigStartValue, id, "0", true);
                    }
                }
                else
                {

                    signalParsingResult |= parsing_SignalName(table[row, columnMapping[DictionaryColumnKey.SignalName.ToString()].ColumnIndex], out string name);

                    signalParsingResult |= parsing_ByteOrder(table[row, columnMapping[DictionaryColumnKey.ByteOrder.ToString()].ColumnIndex], out byte byteOrder);
                    signalParsingResult |= parsing_StartBit(table[row, columnMapping[DictionaryColumnKey.StartBit.ToString()].ColumnIndex], out ushort startBit);
                    signalParsingResult |= parsing_SignalLength(table[row, columnMapping[DictionaryColumnKey.BitLength.ToString()].ColumnIndex], out ushort length);
                    signalParsingResult |= parsing_SignalValueType(table[row, columnMapping[DictionaryColumnKey.DataType.ToString()].ColumnIndex], out DbcValueType valueType);
                    signalParsingResult |= parsing_Factor(table[row, columnMapping[DictionaryColumnKey.Factor.ToString()].ColumnIndex], out double factor);
                    signalParsingResult |= parsing_Offset(table[row, columnMapping[DictionaryColumnKey.Offset.ToString()].ColumnIndex], out double offset);
                    //Non - mandatory parsing content
                    parsing_Description(table[row, columnMapping[DictionaryColumnKey.Description.ToString()].ColumnIndex], out string comment);
                    parsing_MinimumPhysical(table[row, columnMapping[DictionaryColumnKey.MinimumPhysical.ToString()].ColumnIndex], out double minimumPhysical);
                    parsing_MaximumPhysical(table[row, columnMapping[DictionaryColumnKey.MaximumPhysical.ToString()].ColumnIndex], out double maximumPhysical);
                    parsing_initialValue(table[row, columnMapping[DictionaryColumnKey.InitialValue.ToString()].ColumnIndex], out double initialValue);
                    parsing_SignalUnit(table[row, columnMapping[DictionaryColumnKey.Unit.ToString()].ColumnIndex], out string unit);
                    parsing_ValueTableMap(table[row, columnMapping[DictionaryColumnKey.ValueTable.ToString()].ColumnIndex], out IReadOnlyDictionary<int, string> valueTableMap);
                    parsing_SignalReceiver(row, out string[] reveiver);
                    if (signalParsingResult)
                    {
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
            }
            return;
        }
        private bool parsing_ValueTableMap(string valueTableString, out IReadOnlyDictionary<int, string> valueTableMap)
        {
            var tempValueTableMap = new Dictionary<int, string>();

            if (string.IsNullOrWhiteSpace(valueTableString))
            {
                valueTableMap = tempValueTableMap;
                return false;
            }
            var entries = valueTableString.Trim().Replace(",", ":").Replace("：", ":").Replace("，", ":").Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var entry in entries)
            {
                var parts = entry.Split(new[] { ':' }, 2);
                if (parts.Length == 2 && parts[0].StartsWith("0x") && int.TryParse(parts[0].Substring(2), System.Globalization.NumberStyles.HexNumber, null, out int key))
                {
                    tempValueTableMap[key] = parts[1].Trim();
                }
            }
            if (tempValueTableMap.Count > 0)
            {
                valueTableMap = tempValueTableMap;
                return true;
            }
            valueTableMap = tempValueTableMap;
            return false;
        }
        private bool parsing_SignalName(string orignalString, out string name)
        {
            name = string.Empty;
            if (string.IsNullOrEmpty(orignalString))
            {
                return false;
            }
            name = orignalString.Trim();
            return true;
        }
        private bool parsing_Description(string orignalString, out string description)
        {
            description = string.Empty;
            if (string.IsNullOrEmpty(orignalString))
            {
                return false;
            }
            description = orignalString.Trim();
            return true;
        }
        private bool parsing_StartBit(string orignalString, out ushort startBit)
        {
            startBit = 0;
            if (string.IsNullOrEmpty(orignalString))
            {
                return false;
            }
            else if (orignalString.Trim().StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return ushort.TryParse(orignalString.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out startBit);
            }
            else
            {
                return ushort.TryParse(orignalString, out startBit);
            }
        }
        private bool parsing_SignalLength(string orignalString, out ushort signalLength)
        {
            signalLength = 0;
            bool result = false;
            if (string.IsNullOrEmpty(orignalString))
            {
                return false;
            }
            else if (orignalString.Trim().StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                result = ushort.TryParse(orignalString.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out signalLength);
                if (signalLength == 0)
                {
                    return false;
                }
                return result;
            }
            else
            {
                result = ushort.TryParse(orignalString, out signalLength);
                if (signalLength == 0)
                {
                    return false;
                }
                return result;
            }
        }
        private bool parsing_FrameFormat(string orignalString, out string frameFormat)
        {
            frameFormat = "StandardCAN";
            if (orignalString.ToUpper().Contains("J1939"))
            {
                frameFormat = "J1939PG";
                return true;
            }
            else if (orignalString.ToUpper().Contains("NORMAL") || orignalString.ToUpper().Contains("STANDARD"))
            {
                frameFormat = "StandardCAN";
                return true;
            }
            else if (orignalString.ToUpper().Contains("EXTEND"))
            {
                frameFormat = "ExtendedCAN";
                return true;
            }
            return false;
        }
        private bool parsing_MsgSendType(string originalString, out string messageSendType)
        {
            messageSendType = "cyclic";
            if (string.IsNullOrEmpty(originalString))
            {
                return false;
            }
            if (originalString.ToUpper().Contains("cyclic".ToUpper()))
            {
                messageSendType = "cyclic";
                return true;
            }
            else if (originalString.ToUpper().Contains("cyclicIfActive".ToUpper()))
            {
                messageSendType = "cyclicIfActive";
                return true;
            }
            else if (originalString.ToUpper().Contains("noMsgSendType".ToUpper()))
            {
                messageSendType = "noMsgSendType";
                return true;
            }
            return false;
        }
        private bool parsing_MessageTransmitter(int row, out string transmitter)
        {
            for (int col = _nodeStartIndex; col < table_column_count; col++)
            {
                if (!string.IsNullOrEmpty(table[row, col]) && string.Equals(table[row, col].Trim(), "S", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var key in columnMapping.Keys)
                    {
                        if (columnMapping[key].ColumnIndex == col)
                        {
                            transmitter = columnMapping[key].Header;
                        }
                    }
                }
            }
            transmitter = "Vector__XXX";
            return true;
        }

        private bool parsing_MessageId(string orignalString, out uint id)
        {
            id = 0;
            if (string.IsNullOrEmpty(orignalString))
            {
                return false;
            }
            if (orignalString.Trim().StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return uint.TryParse(orignalString.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out id);
            }
            else
            {
                return uint.TryParse(orignalString, out id);
            }
        }
        private bool parsing_MessageCycleTime(string orignalString, out uint cycleTime)
        {
            if (orignalString.Trim().StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return uint.TryParse(orignalString.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out cycleTime);
            }
            else
            {
                return uint.TryParse(orignalString, out cycleTime);
            }
        }
        private bool parsing_SignalValueType(string orignalString, out DbcValueType valueType)
        {
            valueType = DbcValueType.Unsigned;
            bool retVal = false;
            if (string.IsNullOrEmpty(orignalString))
            {
                return false;
            }
            retVal = Enum.TryParse<DbcValueType>(orignalString, out valueType);
            return retVal;
        }
        private bool parsing_SignalReceiver(int row, out string[] signalReceives)
        {
            signalReceives = null;
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
                signalReceives = receivers.ToArray();
                if (signalReceives.Length > 0)
                {
                    return true;
                }
                else
                    return false;

            }
            else
            {

                return false;
            }
        }
        private bool parsing_ByteOrder(string orignalString, out byte byteOrder)
        {
            byteOrder = 1;
            if (string.IsNullOrEmpty(orignalString))
            {
                return false;
            }
            else if (orignalString.Trim().ToUpper().Contains("Intel".ToUpper()))
            {
                byteOrder = 1;
            }
            else if (orignalString.Trim().ToUpper().Contains("Motorola".ToUpper()))
            {
                byteOrder = 0;
            }
            return true;
        }
        private bool parsing_Factor(string orignalString, out double Factor)
        {
            Factor = 0;
            if (string.IsNullOrEmpty(orignalString))
            {
                return false;
            }
            if (orignalString.Trim().StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return double.TryParse(orignalString.Substring(2), System.Globalization.NumberStyles.Float, null, out Factor);
            }
            else
            {
                return double.TryParse(orignalString, out Factor);
            }
        }
        private bool parsing_Offset(string orignalString, out double Offset)
        {
            Offset = 0;
            if (string.IsNullOrEmpty(orignalString))
            {
                return false;
            }
            if (orignalString.Trim().StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return double.TryParse(orignalString.Substring(2), System.Globalization.NumberStyles.Float, null, out Offset);
            }
            else
            {
                return double.TryParse(orignalString, out Offset);
            }
        }
        private bool parsing_MinimumPhysical(string orignalString, out double minimumValue)
        {
            minimumValue = 0;
            if (string.IsNullOrEmpty(orignalString))
            {
                return false;
            }
            if (orignalString.Trim().StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return double.TryParse(orignalString.Substring(2), System.Globalization.NumberStyles.Float, null, out minimumValue);
            }
            else
            {
                return double.TryParse(orignalString, out minimumValue);
            }
        }
        private bool parsing_MaximumPhysical(string orignalString, out double maximumValue)
        {
            maximumValue = 0;
            if (string.IsNullOrEmpty(orignalString))
            {
                return false;
            }
            if (orignalString.Trim().StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return double.TryParse(orignalString.Substring(2), System.Globalization.NumberStyles.Float, null, out maximumValue);
            }
            else
            {
                return double.TryParse(orignalString, out maximumValue);
            }
        }
        private bool parsing_SignalUnit(string orignalString, out string unit)
        {
            unit = string.Empty;
            if (string.IsNullOrEmpty(orignalString))
            {
                return false;
            }
            unit = orignalString.Trim();
            return true;
        }
        private bool parsing_initialValue(string orignalString, out double initialValue)
        {
            initialValue = 0;
            if (string.IsNullOrEmpty(orignalString))
            {
                return false;
            }
            return double.TryParse(orignalString, out initialValue);
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
            _dbcBuilder.AddCustomProperty(CustomPropertyObjectType.Global, new CustomPropertyDefinition(m_observer)
            {
                DataType = CustomPropertyDataType.String,
                Name = ProtocolType,
                StringCustomProperty = new StringCustomPropertyDefinition
                {
                    Default = _protocolType.ToString(),
                }
            });

            switch (_protocolType)
            {
                case DbcProtocolType.J1939:
                    AddCustomJ1939NodeProperty();
                    break;
            }
        }
        private void AddCustomJ1939NodeProperty()
        {
            _dbcBuilder.AddCustomProperty(CustomPropertyObjectType.Node, new CustomPropertyDefinition(m_observer)
            {
                DataType = CustomPropertyDataType.Integer,
                Name = NmStationAddress,
                IntegerCustomProperty = new NumericCustomPropertyDefinition<int>
                {
                    Default = 254,
                    Minimum = 0,
                    Maximum = 255,
                }
            });
            _dbcBuilder.AddCustomProperty(CustomPropertyObjectType.Node, new CustomPropertyDefinition(m_observer)
            {
                DataType = CustomPropertyDataType.Integer,
                Name = NmJ1939IndustryGroup,
                IntegerCustomProperty = new NumericCustomPropertyDefinition<int>
                {
                    Default = 0,
                    Minimum = 0,
                    Maximum = 7,
                }
            });
            _dbcBuilder.AddCustomProperty(CustomPropertyObjectType.Node, new CustomPropertyDefinition(m_observer)
            {
                DataType = CustomPropertyDataType.Integer,
                Name = NmJ1939System,
                IntegerCustomProperty = new NumericCustomPropertyDefinition<int>
                {
                    Default = 0,
                    Minimum = 0,
                    Maximum = 127,
                }
            });
            _dbcBuilder.AddCustomProperty(CustomPropertyObjectType.Node, new CustomPropertyDefinition(m_observer)
            {
                DataType = CustomPropertyDataType.Integer,
                Name = NmJ1939Function,
                IntegerCustomProperty = new NumericCustomPropertyDefinition<int>
                {
                    Default = 0,
                    Minimum = 0,
                    Maximum = 255,
                }
            });
            _dbcBuilder.AddCustomProperty(CustomPropertyObjectType.Node, new CustomPropertyDefinition(m_observer)
            {
                DataType = CustomPropertyDataType.Integer,
                Name = NmJ1939FunctionInstance,
                IntegerCustomProperty = new NumericCustomPropertyDefinition<int>
                {
                    Default = 0,
                    Minimum = 0,
                    Maximum = 7,
                }
            });
            _dbcBuilder.AddCustomProperty(CustomPropertyObjectType.Node, new CustomPropertyDefinition(m_observer)
            {
                DataType = CustomPropertyDataType.Integer,
                Name = NmJ1939ECUInstance,
                IntegerCustomProperty = new NumericCustomPropertyDefinition<int>
                {
                    Default = 0,
                    Minimum = 0,
                    Maximum = 3,
                }
            });
            _dbcBuilder.AddCustomProperty(CustomPropertyObjectType.Node, new CustomPropertyDefinition(m_observer)
            {
                DataType = CustomPropertyDataType.Integer,
                Name = NmJ1939SystemInstance,
                IntegerCustomProperty = new NumericCustomPropertyDefinition<int>
                {
                    Default = 0,
                    Minimum = 0,
                    Maximum = 15,
                }
            });
            _dbcBuilder.AddCustomProperty(CustomPropertyObjectType.Node, new CustomPropertyDefinition(m_observer)
            {
                DataType = CustomPropertyDataType.Integer,
                Name = NmJ1939ManufacturerCode,
                IntegerCustomProperty = new NumericCustomPropertyDefinition<int>
                {
                    Default = 0,
                    Minimum = 0,
                    Maximum = 2047,
                }
            });
            _dbcBuilder.AddCustomProperty(CustomPropertyObjectType.Node, new CustomPropertyDefinition(m_observer)
            {
                DataType = CustomPropertyDataType.Integer,
                Name = NmJ1939IdentityNumber,
                IntegerCustomProperty = new NumericCustomPropertyDefinition<int>
                {
                    Default = 0,
                    Minimum = 0,
                    Maximum = 2097151,
                }
            });
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

        public void SetProtocolType(DbcProtocolType protocolType)
        {
            _protocolType = protocolType;
        }

        public void SetMessageFieldRowStartOffset(int startIndex = 1)
        {
            _messageRowStartOffset = startIndex;
        }

        public void SetNodeRowIndex(int nodeRowIndex = 0)
        {
            _nodeRowIndex = nodeRowIndex;
        }

        public string GetColumnIndexName(DictionaryColumnKey columnKey)
        {
            string retVal = "";
            if (columnMapping.TryGetValue(columnKey.ToString(), out ExcelColumnConfigModel value))
            {
                retVal = convertIndexToColumnName(value.ColumnIndex);
            }
            return retVal;
        }

        public int GetColumnIndex(DictionaryColumnKey columnKey)
        {
            int retVal = -1;
            if (columnMapping.TryGetValue(columnKey.ToString(), out ExcelColumnConfigModel value))
            {
                retVal = value.ColumnIndex;
            }
            return retVal;
        }

        public bool CheckColumnIndexConfiction(out List<int> confictionIndexList)
        {
            confictionIndexList = new List<int>();
            var indexCount = new Dictionary<int, int>();

            foreach (var column in columnMapping.Values)
            {
                if (indexCount.ContainsKey(column.ColumnIndex))
                {
                    indexCount[column.ColumnIndex]++;
                }
                else
                {
                    indexCount[column.ColumnIndex] = 1;
                }
            }

            foreach (var kvp in indexCount)
            {
                if (kvp.Value > 1)
                {
                    confictionIndexList.Add(kvp.Key);
                }
            }

            return confictionIndexList.Count > 0;
        }
    }
}
