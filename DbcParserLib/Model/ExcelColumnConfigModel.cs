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
