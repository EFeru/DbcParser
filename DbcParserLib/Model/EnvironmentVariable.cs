using System.Collections.Generic;

namespace DbcParserLib.Model;

public class EnvironmentVariable
{
    public EnvDataType Type { get; internal set; }
    public string Name { get; internal set; }
    public string Unit { get; internal set; }
    public string Comment { get; internal set; }
    public EnvAccessibility Access { get; internal set; }
    public IReadOnlyDictionary<int, string> ValueTableMap { get; internal set; }
    public IReadOnlyDictionary<string, CustomProperty> CustomProperties => customProperties;
    internal readonly Dictionary<string, CustomProperty> customProperties = new ();
    public NumericEnvironmentVariable<int> IntegerEnvironmentVariable { get; internal set; }
    public NumericEnvironmentVariable<double> FloatEnvironmentVariable { get; internal set; }
    public DataEnvironmentVariable DataEnvironmentVariable { get; internal set; }
}

public class NumericEnvironmentVariable<T>
{
    public T Minimum { get; set; }
    public T Maximum { get; set; }
    public T Default { get; set; }
}

public class DataEnvironmentVariable
{
    public uint Length { get; set; }
}

public enum EnvDataType
{
    Integer,
    Float,
    String,
    Data
}

public enum EnvAccessibility
{
    Unrestricted,
    Read,
    Write,
    ReadWrite
}