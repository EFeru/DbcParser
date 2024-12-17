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
    public enum WriteStatus
    {
        Success,
        PathError,
        FormatError,
        WritePermissionError,
        UnknownError
    }
    public enum ExcelParserState
    {
        Success,
        PathError,
        FormatError,
        ReadPermissionError,
        UnknownError
    }
    public enum UpdateColumnConfigState
    {
        Success,
        ColumnIndexError,
        HeaderError,
        ColumnKeyNotExists,
        UnknownError
    }
}
