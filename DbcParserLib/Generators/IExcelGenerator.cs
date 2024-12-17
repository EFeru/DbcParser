using DbcParserLib.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbcParserLib.Generators
{
    
    public interface IExcelGenerator
    {
        void WriteToFile(Dbc dbc, string path, string sheeName = "Matrix");
        void UpdateColumnConfig(DictionaryColumnKey columnKey, bool? isVisible = null, int? columnIndex = null, string header = null);
    }
}
