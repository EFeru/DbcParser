using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using DbcParserLib.Model;
using NPOI.HSSF.UserModel;
using NPOI.POIFS.Crypt.Dsig;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace DbcParserLib.Generators
{
    public class ExcelGenerator : IExcelGenerator
    {
        private readonly string _xlsExt = ".xls";
        private readonly string _xlsxExt = ".xlsx";

        //For message Group
        private List<int> _messageHeaderIndexList = new List<int>();
        private List<ExcelGroupMessageModel> _messageGroupList = new List<ExcelGroupMessageModel>();

        private string _path = string.Empty;
        private IWorkbook _workbook;
        private ISheet _sheet;

        private ICellStyle _headerCellStyle;
        private IFont _headerFont;

        private ICellStyle _messageHeaderNullCellStyle;
        private ICellStyle _messageHeaderNormalCellStyle;
        private IFont _messageFont;

        private ICellStyle _signalCellStyle;
        private IFont _signalFont;

        private string[,] table;
        private int table_row_count = 0;
        private int table_column_count = 0;
        private IDictionary<string, ExcelColumnConfigModel> columnMapping = new Dictionary<string, ExcelColumnConfigModel>();


        public void GenDefaultDictionary()
        {
            columnMapping.Clear();
            AddColumn(nameof(DictionaryColumnKey.MessageName), "Message\r\nName", 20);
            AddColumn(nameof(DictionaryColumnKey.FrameFormat), "Frame\r\nFormat", 15);
            AddColumn(nameof(DictionaryColumnKey.ID), "Message\r\nID", 15);
            AddColumn(nameof(DictionaryColumnKey.MessageSendType), "Message\r\nSend Type", 18);
            AddColumn(nameof(DictionaryColumnKey.CycleTime), "Cycle\r\nTime", 10);
            AddColumn(nameof(DictionaryColumnKey.DataLength), "Data\r\nLength", 15);
            AddColumn(nameof(DictionaryColumnKey.SignalName), "Signal\r\nName", 25);
            AddColumn(nameof(DictionaryColumnKey.Description), "Description", 40);
            AddColumn(nameof(DictionaryColumnKey.ByteOrder), "Byte\r\nOrder", 10);
            AddColumn(nameof(DictionaryColumnKey.StartBit), "Start\r\nBit", 10);
            AddColumn(nameof(DictionaryColumnKey.BitLength), "Bit\r\nLength", 15);
            AddColumn(nameof(DictionaryColumnKey.DataType), "Sign", 10);
            AddColumn(nameof(DictionaryColumnKey.Factor), "Factor", 10);
            AddColumn(nameof(DictionaryColumnKey.Offset), "Offset", 10);
            AddColumn(nameof(DictionaryColumnKey.MinimumPhysical), "Minimum\r\nPhysical", 15);
            AddColumn(nameof(DictionaryColumnKey.MaximumPhysical), "Maximum\r\nPhysical", 15);
            AddColumn(nameof(DictionaryColumnKey.InitialValue), "Default\r\nValue", 15);
            AddColumn(nameof(DictionaryColumnKey.Unit), "Unit", 10);
            AddColumn(nameof(DictionaryColumnKey.ValueTable), "Value\r\nTable", 25);
        }
        public UpdateColumnConfigState UpdateColumnConfig(string columnKey, bool? isVisible = null, int? columnIndex = null, string header = null, double columnWidth = 0)
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
            columnConfig.IsVisible = isVisible.Value;
            columnConfig.ColumnIndex = columnIndex.Value;
            columnConfig.Header = header;
            columnConfig.ColumnWidth = columnWidth;
            return UpdateColumnConfigState.Success;
        }
        public UpdateColumnConfigState UpdateColumnConfig(DictionaryColumnKey columnKey, bool? isVisible = null, int? columnIndex = null, string header = null, double columnWidth = 0)
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
            columnConfig.IsVisible = isVisible.Value;
            columnConfig.ColumnIndex = columnIndex.Value;
            columnConfig.Header = header;
            columnConfig.ColumnWidth = columnWidth;
            return UpdateColumnConfigState.Success;
        }
        public UpdateColumnConfigState UpdateColumnConfig(DictionaryColumnKey columnKey, bool isVisible)
        {
            if (!columnMapping.ContainsKey(columnKey.ToString()))
            {
                return UpdateColumnConfigState.ColumnKeyNotExists;
            }
            var columnConfig = columnMapping[columnKey.ToString()];
            columnConfig.IsVisible = isVisible;
            return UpdateColumnConfigState.Success;
        }
        public UpdateColumnConfigState UpdateColumnConfig(string columnKey, bool isVisible)
        {
            if (!columnMapping.ContainsKey(columnKey))
            {
                return UpdateColumnConfigState.ColumnKeyNotExists;
            }
            var columnConfig = columnMapping[columnKey];
            columnConfig.IsVisible = isVisible;
            return UpdateColumnConfigState.Success;
        }
        public UpdateColumnConfigState UpdateColumnConfig(DictionaryColumnKey columnKey,  int columnIndex)
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
        public UpdateColumnConfigState UpdateColumnConfig(DictionaryColumnKey columnKey,  string header)
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
        public UpdateColumnConfigState UpdateColumnConfig(DictionaryColumnKey columnKey, double columnWidth = 0)
        {
            if (!columnMapping.ContainsKey(columnKey.ToString()))
            {
                return UpdateColumnConfigState.ColumnKeyNotExists;
            }
            var columnConfig = columnMapping[columnKey.ToString()];

            columnConfig.ColumnWidth = columnWidth;
            return UpdateColumnConfigState.Success;
        }
        public UpdateColumnConfigState UpdateColumnConfig(string columnKey, double columnWidth = 0)
        {
            if (!columnMapping.ContainsKey(columnKey))
            {
                return UpdateColumnConfigState.ColumnKeyNotExists;
            }
            var columnConfig = columnMapping[columnKey];

            columnConfig.ColumnWidth = columnWidth;
            return UpdateColumnConfigState.Success;
        }
        public void AddColumnNode(string columnKey)
        {
            if (!columnMapping.ContainsKey(columnKey))
            {
                columnMapping.Add(columnKey, new ExcelColumnConfigModel() { Header = columnKey, IsVisible = true, ColumnIndex = columnMapping.Count });
            }
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

        public ExcelGenerator()
        {
            GenDefaultDictionary();
        }


        public WriteStatus WriteToFile(Dbc dbc, string path, string sheeName = "Matrix")
        {
            try
            {
                if (!tryCreateWorkbook(path, out _workbook))
                {
                    return WriteStatus.FormatError;
                }

                _path = path;
                table_row_count = calculateExcelRowsCount(dbc);
                table_column_count = calculateExcelColumnCount(dbc);
                table = new string[table_row_count, table_column_count];
                writeColumnHeader();
                writeMessages(dbc);
                // Write the table to the Excel file
                WriteTableToExcel(sheeName);
                // Save the workbook to the file
                using (var fileData = new FileStream(_path, FileMode.Create))
                {
                    _workbook.Write(fileData);
                }
                return WriteStatus.Success;
            }
            catch (DirectoryNotFoundException)
            {
                return WriteStatus.PathError;
            }
            catch (IOException ex)
            {
                return WriteStatus.WritePermissionError;
            }
            catch (Exception ex)
            {
                return WriteStatus.UnknownError;
            }
        }

        private void SetColumnWidths()
        {
            foreach (var key in columnMapping.Keys)
            {
                var value = columnMapping[key];
                if (value.ColumnIndex < table_column_count)
                {
                    if (value.ColumnWidth > 0)
                    {
                        _sheet.SetColumnWidth(value.ColumnIndex, (value.ColumnWidth * 256)); // Set custom column width
                    }
                    else
                    {
                        _sheet.AutoSizeColumn(value.ColumnIndex); // Set default column width
                    }
                }
            }
        }

        private void WriteTableToExcel(string sheetName)
        {
            _sheet = _workbook.CreateSheet(sheetName);
            _sheet.CreateFreezePane(0, 1); // Freeze the first row
            bool isMessageHeader = false;
            for (int i = 0; i < table_row_count; i++)
            {
                IRow row = _sheet.CreateRow(i);
                //Setting Cell Style
                isMessageHeader = isMessageHeaderLine(i);
                if (isMessageHeader)
                {
                    _messageHeaderIndexList.Add(i);
                }
                for (int j = 0; j < table_column_count; j++)
                {
                    ICell cell = row.CreateCell(j);
                    cell.SetCellValue(table[i, j]);

                    if (i == 0)
                    {
                        cell.CellStyle = _headerCellStyle;
                    }
                    else
                    {
                        if (isMessageHeader)
                        {
                            cell.CellStyle = _messageHeaderNormalCellStyle;
                        }
                        else
                        {
                            if (isSignalColumn(j))
                            {
                                cell.CellStyle = _signalCellStyle;
                            }
                        }
                    }
                }
            }
            _messageHeaderIndexList.Add(table_row_count);
            _messageGroupList = generateGroupData(_messageHeaderIndexList);

            foreach (ExcelGroupMessageModel rowGroup in _messageGroupList)
            {
                _sheet.GroupRow(rowGroup.StartIndex, rowGroup.EndIndex);
                //_sheet.SetRowGroupCollapsed(rowGroup.StartIndex, true);
            }
            // Set column widths based on the dictionary
            SetColumnWidths();
        }
        static List<ExcelGroupMessageModel> generateGroupData(List<int> data)
        {
            if (data == null || data.Count < 2)
                return new List<ExcelGroupMessageModel>();
            List<ExcelGroupMessageModel> groupDataList = new List<ExcelGroupMessageModel>();

            for (int i = 0; i < data.Count - 1; i++)
            {
                ExcelGroupMessageModel group = new ExcelGroupMessageModel
                {
                    StartIndex = data[i] + 1,
                    EndIndex = data[i + 1] - 1
                };

                groupDataList.Add(group);
            }
            return groupDataList;
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
        private bool isSignalColumn(int column)
        {
            var excludedKeys = new HashSet<string>
            {
                nameof(DictionaryColumnKey.MessageName),
                nameof(DictionaryColumnKey.FrameFormat),
                nameof(DictionaryColumnKey.ID),
                nameof(DictionaryColumnKey.MessageSendType),
                nameof(DictionaryColumnKey.CycleTime),
                nameof(DictionaryColumnKey.DataLength)
            };

            foreach (var key in columnMapping.Keys)
            {
                if (!excludedKeys.Contains(key))
                {
                    var value = columnMapping[key];
                    if (value.ColumnIndex == column)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private int calculateExcelRowsCount(Dbc dbc)
        {
            int retVal = 0;
            if (dbc != null)
            {
                foreach (Message message in dbc.Messages)
                {
                    //Message row
                    retVal++;
                    // Signal row
                    retVal += message.Signals.Count();
                }
            }
            //Header row
            retVal++;
            return retVal;
        }
        private int calculateExcelColumnCount(Dbc dbc)
        {
            int retVal = 0;
            foreach (Node node in dbc.Nodes)
            {
                AddColumnNode(node.Name);
            }
            retVal += columnMapping.Count();
            return retVal;
        }
        public void WriteToFile(Dbc dbc)
        {

        }

        private bool tryCreateWorkbook(string path, out IWorkbook workbook)
        {
            workbook = null;

            if (string.IsNullOrEmpty(path))
            {
                Console.WriteLine("Path is null or empty.");
                return false;
            }

            string extension = Path.GetExtension(path);
            if (string.Equals(extension, _xlsExt, StringComparison.OrdinalIgnoreCase))
            {
                workbook = new HSSFWorkbook();
                CreateHeaderCellStyle();
                CreateMessageHeaderStyleNull();
                CreateMessageHeaderStyleNormal();
                CreateSignalStyle();
                return true;
            }
            else if (string.Equals(extension, _xlsxExt, StringComparison.OrdinalIgnoreCase))
            {
                workbook = new XSSFWorkbook();
                CreateHeaderCellStyle();
                CreateMessageHeaderStyleNull();
                CreateMessageHeaderStyleNormal();
                CreateSignalStyle();
                return true;
            }
            else
            {
                Console.WriteLine("Unsupported file extension: " + extension);
                return false;
            }
        }
        private void CreateCell(IRow CurrentRow, int CellIndex, string Value, HSSFCellStyle Style)
        {
            ICell Cell = CurrentRow.CreateCell(CellIndex);
            Cell.SetCellValue(Value);
            Cell.CellStyle = Style;
        }
        public bool ConvertToArray(Dbc dbc)
        {
            bool retVal = false;
            table = new string[table_row_count, table_column_count];

            return retVal;
        }
        private void writeColumnHeader()
        {
            foreach (var key in columnMapping.Keys)
            {
                var value = columnMapping[key];
                if (value.ColumnIndex < table_column_count)
                {
                    table[0, value.ColumnIndex] = value.Header;
                }
            }
        }
        private void writeMessages(Dbc dbc)
        {
            int CurrentLine = 1;
            foreach (var message in dbc.Messages)
            {
                writeMessageline(CurrentLine, message);
                CurrentLine++;
                foreach (Signal signal in message.Signals)
                {
                    writeSignalline(CurrentLine, signal);
                    CurrentLine++;
                }
            }
        }
        private void writeMessageline(int currentLine, Message message)
        {
            //MessageName
            if (columnMapping.TryGetValue(DictionaryColumnKey.MessageName.ToString(), out ExcelColumnConfigModel MessageNameValue))
            {
                table[currentLine, MessageNameValue.ColumnIndex] = message.Name;
            }
            //FrameFormat
            if (columnMapping.TryGetValue(DictionaryColumnKey.FrameFormat.ToString(), out ExcelColumnConfigModel FrameFormatValue))
            {
                if (message.CustomProperties.TryGetValue("VFrameFormat", out CustomProperty MessageFrameFormatValue))
                {
                    table[currentLine, FrameFormatValue.ColumnIndex] = MessageFrameFormatValue.EnumCustomProperty.Value ?? (message.IsExtID ? "ExtendedCAN" : "StandardCAN");
                }
                else
                {
                    table[currentLine, FrameFormatValue.ColumnIndex] = message.IsExtID ? "ExtendedCAN" : "StandardCAN";
                }
            }
            //ID
            if (columnMapping.TryGetValue(DictionaryColumnKey.ID.ToString(), out ExcelColumnConfigModel IDValue))
            {
                table[currentLine, IDValue.ColumnIndex] = $"0x{message.ID:X}";
            }
            //MessageSendType
            if (columnMapping.TryGetValue(DictionaryColumnKey.MessageSendType.ToString(), out ExcelColumnConfigModel MessageSendTypeValue))
            {
                if (message.CustomProperties.TryGetValue("GenMsgSendType", out CustomProperty GenMsgSendTypeValue))
                {
                    table[currentLine, MessageSendTypeValue.ColumnIndex] = GenMsgSendTypeValue.EnumCustomProperty.Value ?? "cyclic";
                }
                else
                {
                    table[currentLine, MessageSendTypeValue.ColumnIndex] = "cyclic";
                }
            }
            //CycleTime
            if (columnMapping.TryGetValue(DictionaryColumnKey.CycleTime.ToString(), out ExcelColumnConfigModel CycleTimeValue))
            {
                if (message.CustomProperties.TryGetValue("GenMsgCycleTime", out CustomProperty GenMsgSendTypeValue))
                {
                    table[currentLine, CycleTimeValue.ColumnIndex] = GenMsgSendTypeValue.IntegerCustomProperty.Value.ToString() ?? "0";
                }
                else
                {
                    table[currentLine, CycleTimeValue.ColumnIndex] = "0";
                }
            }
            //DataLength
            if (columnMapping.TryGetValue(DictionaryColumnKey.DataLength.ToString(), out ExcelColumnConfigModel DataLengthValue))
            {
                table[currentLine, DataLengthValue.ColumnIndex] = message.DLC.ToString();
            }
            //Description
            if (columnMapping.TryGetValue(DictionaryColumnKey.Description.ToString(), out ExcelColumnConfigModel DescriptionValue))
            {
                table[currentLine, DescriptionValue.ColumnIndex] = message.Comment ?? string.Empty;
            }
            //Node
            if (columnMapping.TryGetValue(message.Transmitter, out ExcelColumnConfigModel TransmitterValue))
            {
                table[currentLine, TransmitterValue.ColumnIndex] = "S";
            }

        }
        private void writeSignalline(int currentLine, Signal signal)
        {
            //SignalName
            if (columnMapping.TryGetValue(DictionaryColumnKey.SignalName.ToString(), out ExcelColumnConfigModel SignalNameValue))
            {
                table[currentLine, SignalNameValue.ColumnIndex] = signal.Name;
            }
            //Description
            if (columnMapping.TryGetValue(DictionaryColumnKey.Description.ToString(), out ExcelColumnConfigModel DescriptionValue))
            {
                table[currentLine, DescriptionValue.ColumnIndex] = signal.Comment ?? string.Empty;
            }
            //ByteOrder
            if (columnMapping.TryGetValue(DictionaryColumnKey.ByteOrder.ToString(), out ExcelColumnConfigModel ByteOrderValue))
            {
                table[currentLine, ByteOrderValue.ColumnIndex] = signal.ByteOrder == 1 ? "Intel" : "Motorola";
            }
            //StartBit
            if (columnMapping.TryGetValue(DictionaryColumnKey.StartBit.ToString(), out ExcelColumnConfigModel StartBitValue))
            {
                table[currentLine, StartBitValue.ColumnIndex] = signal.StartBit.ToString();
            }
            //BitLength
            if (columnMapping.TryGetValue(DictionaryColumnKey.BitLength.ToString(), out ExcelColumnConfigModel BitLengthValue))
            {
                table[currentLine, BitLengthValue.ColumnIndex] = signal.Length.ToString();
            }
            //Sign
            if (columnMapping.TryGetValue(DictionaryColumnKey.DataType.ToString(), out ExcelColumnConfigModel SignValue))
            {
                table[currentLine, SignValue.ColumnIndex] = signal.ValueType.ToString();
            }
            //Factor
            if (columnMapping.TryGetValue(DictionaryColumnKey.Factor.ToString(), out ExcelColumnConfigModel FactorValue))
            {
                table[currentLine, FactorValue.ColumnIndex] = signal.Factor.ToString();
            }
            //Offset
            if (columnMapping.TryGetValue(DictionaryColumnKey.Offset.ToString(), out ExcelColumnConfigModel OffsetValue))
            {
                table[currentLine, OffsetValue.ColumnIndex] = signal.Offset.ToString();
            }
            //MinimumPhysical
            if (columnMapping.TryGetValue(DictionaryColumnKey.MinimumPhysical.ToString(), out ExcelColumnConfigModel MinimumPhysicalValue))
            {
                table[currentLine, MinimumPhysicalValue.ColumnIndex] = signal.Minimum.ToString();
            }
            //MaximumPhysical
            if (columnMapping.TryGetValue(DictionaryColumnKey.MaximumPhysical.ToString(), out ExcelColumnConfigModel MaximumPhysicalValue))
            {
                table[currentLine, MaximumPhysicalValue.ColumnIndex] = signal.Maximum.ToString();
            }
            //DefaultValue
            if (columnMapping.TryGetValue(DictionaryColumnKey.InitialValue.ToString(), out ExcelColumnConfigModel DefaultValueValue))
            {
                table[currentLine, DefaultValueValue.ColumnIndex] = signal.InitialValue.ToString();
            }
            //Unit
            if (columnMapping.TryGetValue(DictionaryColumnKey.Unit.ToString(), out ExcelColumnConfigModel UnitValue))
            {
                table[currentLine, UnitValue.ColumnIndex] = signal.Unit;
            }
            //ValueTable
            if (columnMapping.TryGetValue(DictionaryColumnKey.ValueTable.ToString(), out ExcelColumnConfigModel ValueTableValue))
            {
                string formattedValueTable = string.Join("\r\n", signal.ValueTableMap.Select(x => $"0x{x.Key:X}:{x.Value}"));
                table[currentLine, ValueTableValue.ColumnIndex] = formattedValueTable;
            }
            //Node
            foreach (var node in signal.Receiver)
            {
                if (columnMapping.TryGetValue(node, out ExcelColumnConfigModel NodeValue))
                {
                    table[currentLine, NodeValue.ColumnIndex] = "R";
                }
            }
        }
        private ICellStyle CreateHeaderCellStyle()
        {
            _headerCellStyle = _workbook.CreateCellStyle();

            _headerFont = _workbook.CreateFont();
            _headerFont.IsBold = true;
            _headerFont.FontHeightInPoints = 14;

            _headerCellStyle.SetFont(_headerFont);
            _headerCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            _headerCellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;

            // Set background color to Cyan
            _headerCellStyle.FillPattern = FillPattern.SolidForeground;
            _headerCellStyle.FillForegroundColor = IndexedColors.LightBlue.Index;

            // Set border to thin solid line
            _headerCellStyle.BorderTop = BorderStyle.Thin;
            _headerCellStyle.BorderBottom = BorderStyle.Thin;
            _headerCellStyle.BorderLeft = BorderStyle.Thin;
            _headerCellStyle.BorderRight = BorderStyle.Thin;

            // Enable text wrapping
            _headerCellStyle.WrapText = true;

            return _headerCellStyle;
        }

        private ICellStyle CreateMessageHeaderStyleNull()
        {
            _messageHeaderNullCellStyle = _workbook.CreateCellStyle();
            _messageFont = _workbook.CreateFont();
            _messageFont.IsBold = true;
            _messageFont.FontHeightInPoints = 10;
            _messageHeaderNullCellStyle.SetFont(_messageFont);
            _messageHeaderNullCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            _messageHeaderNullCellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            _messageHeaderNullCellStyle.BorderTop = BorderStyle.Thin;
            _messageHeaderNullCellStyle.BorderBottom = BorderStyle.Thin;
            _messageHeaderNullCellStyle.FillPattern = FillPattern.SolidForeground;
            _messageHeaderNullCellStyle.FillForegroundColor = IndexedColors.Yellow.Index;
            _messageHeaderNullCellStyle.WrapText = true;
            return _messageHeaderNullCellStyle;
        }
        private ICellStyle CreateMessageHeaderStyleNormal()
        {
            _messageHeaderNormalCellStyle = _workbook.CreateCellStyle();
            _messageFont = _workbook.CreateFont();
            _messageFont.IsBold = true;
            _messageFont.FontHeightInPoints = 10;
            _messageHeaderNormalCellStyle.SetFont(_messageFont);
            _messageHeaderNormalCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            _messageHeaderNormalCellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            _messageHeaderNormalCellStyle.BorderTop = BorderStyle.Thin;
            _messageHeaderNormalCellStyle.BorderBottom = BorderStyle.Thin;
            _messageHeaderNormalCellStyle.BorderLeft = BorderStyle.Thin;
            _messageHeaderNormalCellStyle.BorderRight = BorderStyle.Thin;
            _messageHeaderNormalCellStyle.FillPattern = FillPattern.SolidForeground;
            _messageHeaderNormalCellStyle.FillForegroundColor = IndexedColors.Yellow.Index;
            _messageHeaderNormalCellStyle.WrapText = true;
            return _messageHeaderNormalCellStyle;
        }

        private ICellStyle CreateSignalStyle()
        {
            _signalCellStyle = _workbook.CreateCellStyle();
            _signalFont = _workbook.CreateFont();
            _signalFont.IsBold = false;
            _signalFont.FontHeightInPoints = 10;
            _signalCellStyle.SetFont(_signalFont);
            _signalCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
            _signalCellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            _signalCellStyle.BorderTop = BorderStyle.Thin;
            _signalCellStyle.BorderBottom = BorderStyle.Thin;
            _signalCellStyle.BorderLeft = BorderStyle.Thin;
            _signalCellStyle.BorderRight = BorderStyle.Thin;
            _signalCellStyle.FillPattern = FillPattern.SolidForeground;
            _signalCellStyle.FillForegroundColor = IndexedColors.LightGreen.Index;
            _signalCellStyle.WrapText = true;
            return _signalCellStyle;
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

        public bool CheckColumnIndexConfiction(int columnIndex)
        {
            foreach (var column in columnMapping.Values)
            {
                if (column.ColumnIndex == columnIndex)
                {
                    return true;
                }
            }
            return false;
        }

        public IDictionary<string, ExcelColumnConfigModel> GetColumnConfiguration()
        {
            return columnMapping;
        }
    }
}
