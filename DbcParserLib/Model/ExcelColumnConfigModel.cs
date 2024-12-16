using System;
using System.Collections.Generic;
using System.Text;

namespace DbcParserLib.Model
{
    public class ExcelColumnConfigModel
    {
        public string Header { get; set; }
        public bool IsVisible { get; set; }
        public int ColumnIndex { get; set; }
        public double ColumnWidth { get; set; } 
    }
}
