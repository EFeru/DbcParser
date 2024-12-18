using DbcParserLib.Model;
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
        private string[,] table;
        private int table_row_count = 0;
        private int table_column_count = 0;
        private IDictionary<string, ExcelColumnConfigModel> columnMapping = new Dictionary<string, ExcelColumnConfigModel>();
        private int _nodeStartIndex = 0;
        private IEnumerable<Node> _nodes;
        private IEnumerable<Message> _messages;
        private IEnumerable<EnvironmentVariable> _environmentVariables;
        private IEnumerable<CustomProperty> globalProperties;

        public ExcelParser()
        {
            GenDefaultDictionary();
        }
        public ExcelParser(IDictionary<DictionaryColumnKey, ExcelColumnConfigModel> excelTitleDictionary,int nodeStartColumnIndex)
        {
            columnMapping.Clear();
            foreach (var key in excelTitleDictionary.Keys) 
            {
               if( excelTitleDictionary.TryGetValue(key,out ExcelColumnConfigModel model))
                {
                    AddColumn(key.ToString(), model.Header);
                    UpdateColumnConfig(key.ToString(),model.ColumnIndex);
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
        private IEnumerable<Node> ParseNodesFromTable()
        {
            var nodes = new List<Node>();
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
                        nodes.Add(node);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message.ToString());
            }
            return nodes;
        }
        private IEnumerable<Message> ParseMessageFromTable()
        {
            var messages = new List<Message>();
            for (int row = 1; row < table_row_count; row++)
            {
                if (isMessageHeaderLine(row))
                {
                    var message = new Message
                    {
                        Name = table[row, columnMapping[DictionaryColumnKey.MessageName.ToString()].ColumnIndex],
                        ID = Convert.ToUInt32(table[row, columnMapping[DictionaryColumnKey.ID.ToString()].ColumnIndex]),
                        //FrameFormat = table[row, columnMapping[DictionaryColumnKey.FrameFormat.ToString()].ColumnIndex],
                        //SendType = table[row, columnMapping[DictionaryColumnKey.MessageSendType.ToString()].ColumnIndex],
                        //CycleTime = Convert.ToUInt32(table[row, columnMapping[DictionaryColumnKey.CycleTime.ToString()].ColumnIndex]),
                        //DataLength = Convert.ToByte(table[row, columnMapping[DictionaryColumnKey.DataLength.ToString()].ColumnIndex]),
                        //Description = table[row, columnMapping[DictionaryColumnKey.Description.ToString()].ColumnIndex],
                    };
                    messages.Add(message);
                }
            }
            return messages;
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
