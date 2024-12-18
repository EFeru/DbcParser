using DbcParserLib.Model;

namespace DbcParserLib.Parsers
{
    public interface IExcelParser
    {
        ExcelParserState ParseFromPath(string path, out Dbc dbc);
        ExcelParserState ParseFromPath(string path, string sheetName, out Dbc dbc);
        ExcelParserState ParseFromPath(string path, int sheetIndex, out Dbc dbc);

        UpdateColumnConfigState UpdateColumnConfig(DictionaryColumnKey columnKey, int? columnIndex = null, string header = null);
        UpdateColumnConfigState UpdateColumnConfig(string columnKey, int? columnIndex = null, string header = null);
        UpdateColumnConfigState UpdateColumnConfig(DictionaryColumnKey columnKey, int columnIndex);
        UpdateColumnConfigState UpdateColumnConfig(string columnKey, int columnIndex);
        UpdateColumnConfigState UpdateColumnConfig(DictionaryColumnKey columnKey, string header);
        UpdateColumnConfigState UpdateColumnConfig(string columnKey, string header);

        void SetNodeStartIndex(int nodeStartIndex);
        void SetNodeStartIndex(string excelColoumnName);
        int GetNodeStartIndex();
    }
}
