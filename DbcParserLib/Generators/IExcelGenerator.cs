using DbcParserLib.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbcParserLib.Generators
{

    public interface IExcelGenerator
    {
        WriteStatus WriteToFile(Dbc dbc, string path, string sheeName = "Matrix");

        UpdateColumnConfigState UpdateColumnConfig(DictionaryColumnKey columnKey, bool? isVisible = null, int? columnIndex = null, string header = null, double columnWidth = 0);
        UpdateColumnConfigState UpdateColumnConfig(string columnKey, bool? isVisible = null, int? columnIndex = null, string header = null, double columnWidth = 0);
        UpdateColumnConfigState UpdateColumnConfig(DictionaryColumnKey columnKey, bool isVisible);
        UpdateColumnConfigState UpdateColumnConfig(string columnKey, bool isVisible);
        UpdateColumnConfigState UpdateColumnConfig(DictionaryColumnKey columnKey, int columnIndex);
        UpdateColumnConfigState UpdateColumnConfig(string columnKey, int columnIndex);
        UpdateColumnConfigState UpdateColumnConfig(DictionaryColumnKey columnKey, string header);
        UpdateColumnConfigState UpdateColumnConfig(string columnKey, string header);
        UpdateColumnConfigState UpdateColumnConfig(DictionaryColumnKey columnKey, double columnWidth = 0);
        UpdateColumnConfigState UpdateColumnConfig(string columnKey, double columnWidth = 0);

        bool CheckColumnIndexConfiction(out List<int> confictionIndexList);
        bool CheckColumnIndexConfiction(int columnIndex);
        IDictionary<string, ExcelColumnConfigModel> GetColumnConfiguration();

    }
}
