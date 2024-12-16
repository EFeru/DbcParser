using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using DbcParserLib.Model;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.Util;
using NPOI.XSSF.UserModel;
using SixLabors.ImageSharp;

namespace DbcParserLib.Generators
{
    public class ExcelGenerator : IExcelGenerator
    {
        private readonly string _xlsExt = ".xls";
        private readonly string _xlsxExt = ".xlsx";

        private string _path = string.Empty;
        private IWorkbook _workbook;
        private ISheet _sheet;
        private string[,] table;
        private IDictionary<string, ExcelColumnConfigModel> columnMapping = new Dictionary<string, ExcelColumnConfigModel>();
        private int table_row_count = 0;
        private int table_column_count = 0;

        public void GenDefaultDictionary()
        {
            columnMapping.Clear();
            AddColumn("MessageName", "Message Name");
            AddColumn("FrameFormat", "Frame Format");
            AddColumn("ID", "Message ID");
            AddColumn("MessageSendType", "Message Send Type");
            AddColumn("CycleTime", "Cycle Time");
            AddColumn("DataLength", "Data Length");
            AddColumn("SignalName", "Signal Name");
            AddColumn("Description", "Description");
            AddColumn("ByteOrder", "Byte Order");
            AddColumn("StartBit", "Start Bit");
            AddColumn("BitLength", "Bit Length");
            AddColumn("Sign", "Sign");
            AddColumn("Factor", "Factor");
            AddColumn("Offset", "Offset");
            AddColumn("MinimumPhysical", "Minimum Physical");
            AddColumn("MaximumPhysical", "Maximum Physical");
            AddColumn("DefaultValue", "Default Value");
            AddColumn("Unit", "Unit");
            AddColumn("ValueTable", "Value Table");
            //columnMapping = new Dictionary<string, ExcelColumnConfigModel>
            //{
            //    {"MessageName", new ExcelColumnConfigModel(){ IsVisible = true, ColumnIndex = 0, Header = "Message Name"}},
            //    {"FrameFormat", new ExcelColumnConfigModel(){ IsVisible = true, ColumnIndex = 1, Header = "Frame Format"}},
            //    {"ID", new ExcelColumnConfigModel(){ IsVisible = true, ColumnIndex = 2, Header = "Message ID"}},
            //    {"MessageSendType", new ExcelColumnConfigModel(){ IsVisible = true, ColumnIndex = 3, Header = "Message Send Type"}},
            //    {"CycleTime", new ExcelColumnConfigModel(){ IsVisible = true, ColumnIndex = 4, Header = "Cycle Time"}},
            //    {"DataLength", new ExcelColumnConfigModel(){ IsVisible = true, ColumnIndex = 5, Header = "Data Length"}},
            //    {"SignalName", new ExcelColumnConfigModel(){ IsVisible = true, ColumnIndex = 6, Header = "Signal Name"}},
            //    {"Description", new ExcelColumnConfigModel(){ IsVisible = true, ColumnIndex = 7, Header = "Description"}},
            //    {"ByteOrder", new ExcelColumnConfigModel(){ IsVisible = true, ColumnIndex = 8, Header = "Byte Order"}},
            //    {"StartBit", new ExcelColumnConfigModel(){ IsVisible = true, ColumnIndex = 9, Header = "Star tBit"}},
            //    {"BitLength", new ExcelColumnConfigModel(){ IsVisible = true, ColumnIndex = 10, Header = "Bit Length"}},
            //    {"Sign", new ExcelColumnConfigModel(){ IsVisible = true, ColumnIndex = 11, Header = "Sign"}},
            //    {"Factor", new ExcelColumnConfigModel(){ IsVisible = true, ColumnIndex = 12, Header = "Factor"}},
            //    {"Offset", new ExcelColumnConfigModel(){ IsVisible = true, ColumnIndex = 13, Header = "Offset"}},
            //    {"MinimumPhysical", new ExcelColumnConfigModel(){ IsVisible = true, ColumnIndex = 14, Header = "Minimum Physical"}},
            //    {"MaximumPhysical", new ExcelColumnConfigModel(){ IsVisible = true, ColumnIndex = 15, Header = "Maximum Physical"}},
            //    {"DefaultValue", new ExcelColumnConfigModel(){ IsVisible = true, ColumnIndex = 16, Header = "Default Value"}},
            //    {"Unit", new ExcelColumnConfigModel(){ IsVisible = true, ColumnIndex = 17, Header = "Unit"}},
            //    {"ValueTable", new ExcelColumnConfigModel(){ IsVisible = true, ColumnIndex = 18, Header = "Value Table"}},
            //};
        }
        public void UpdateColumnConfig(string columnKey, bool? isVisible = null, int? columnIndex = null, string header = null)
        {
            if (columnMapping.ContainsKey(columnKey))
            {
                var columnConfig = columnMapping[columnKey];
                if (isVisible.HasValue)
                {
                    columnConfig.IsVisible = isVisible.Value;
                }

                if (columnIndex.HasValue)
                {
                    columnConfig.ColumnIndex = columnIndex.Value;
                }

                if (!string.IsNullOrEmpty(header))
                {
                    columnConfig.Header = header;
                }
            }
            else
            {
                Console.WriteLine($"Column key '{columnKey}' not found in the dictionary.");
            }
        }
        public void UpdateColumnConfig(DictionaryColumnKey columnKey, bool? isVisible = null, int? columnIndex = null, string header = null)
        {
            string key = columnKey.ToString();
            if (columnMapping.ContainsKey(key))
            {
                var columnConfig = columnMapping[key];
                if (isVisible.HasValue)
                {
                    columnConfig.IsVisible = isVisible.Value;
                }

                if (columnIndex.HasValue)
                {
                    columnConfig.ColumnIndex = columnIndex.Value;
                }

                if (!string.IsNullOrEmpty(header))
                {
                    columnConfig.Header = header;
                }
            }
            else
            {
                Console.WriteLine($"Column key '{columnKey}' not found in the dictionary.");
            }
        }
        public void AddColumnNode(string columnKey)
        {
            if (!columnMapping.ContainsKey(columnKey))
            {
                columnMapping.Add(columnKey, new ExcelColumnConfigModel() { Header = columnKey, IsVisible = true, ColumnIndex = columnMapping.Count });
            }
        }
        public void AddColumn(string columnKey, string Header = "", bool visible = true)
        {
            if (!columnMapping.ContainsKey(columnKey))
            {
                columnMapping.Add(columnKey, new ExcelColumnConfigModel()
                {
                    Header = string.IsNullOrEmpty(Header) ? columnKey : Header,
                    IsVisible = visible,
                    ColumnIndex = columnMapping.Count
                });
            }
        }
        public ExcelGenerator(string path)
        {
            if (tryCreateWorkbook(path, out _workbook))
            {
                _path = path;
            }
            GenDefaultDictionary();
        }

        public ExcelGenerator()
        {
            GenDefaultDictionary();
        }
        public void WriteToFile(Dbc dbc, string path)
        {
            if (tryCreateWorkbook(path, out _workbook))
            {
                _path = path;
            }
            table_row_count = calculateExcelRowsCount(dbc);
            table_column_count = calculateExcelColumnCount(dbc);
            table = new string[table_row_count, table_column_count];
            writeColumnHeader();
            writeMessages(dbc);
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
                HSSFFont myFont = (HSSFFont)workbook.CreateFont();
                myFont.FontHeightInPoints = 11;
                myFont.FontName = "Tahoma";

                // Defining a border
                HSSFCellStyle borderedCellStyle = (HSSFCellStyle)workbook.CreateCellStyle();
                borderedCellStyle.SetFont(myFont);
                borderedCellStyle.BorderLeft = BorderStyle.Medium;
                borderedCellStyle.BorderTop = BorderStyle.Medium;
                borderedCellStyle.BorderRight = BorderStyle.Medium;
                borderedCellStyle.BorderBottom = BorderStyle.Medium;
                borderedCellStyle.VerticalAlignment = VerticalAlignment.Center;

                ISheet Sheet = workbook.CreateSheet("Report");
                //Creat The Headers of the excel
                IRow HeaderRow = Sheet.CreateRow(0);
                //Create The Actual Cells
                CreateCell(HeaderRow, 0, "Batch Name", borderedCellStyle);
                CreateCell(HeaderRow, 1, "RuleID", borderedCellStyle);
                CreateCell(HeaderRow, 2, "Rule Type", borderedCellStyle);
                CreateCell(HeaderRow, 3, "Code Message Type", borderedCellStyle);
                CreateCell(HeaderRow, 4, "Severity", borderedCellStyle);
                // This Where the Data row starts from
                int RowIndex = 1;
                return true;
            }
            else if (string.Equals(extension, _xlsxExt, StringComparison.OrdinalIgnoreCase))
            {
                workbook = new XSSFWorkbook();
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
            if (columnMapping.TryGetValue(DictionaryColumnKey.Sign.ToString(), out ExcelColumnConfigModel SignValue))
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
            if (columnMapping.TryGetValue(DictionaryColumnKey.DefaultValue.ToString(), out ExcelColumnConfigModel DefaultValueValue))
            {
                table[currentLine, DefaultValueValue.ColumnIndex] = signal.Maximum.ToString();
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
        }
        public enum DictionaryColumnKey
        {
            MessageName,
            FrameFormat,
            ID,
            MessageSendType,
            CycleTime,
            DataLength,
            SignalName,
            Description,
            ByteOrder,
            StartBit,
            BitLength,
            Sign,
            Factor,
            Offset,
            MinimumPhysical,
            MaximumPhysical,
            DefaultValue,
            Unit,
            ValueTable,
        }
    }
}
