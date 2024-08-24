using System.Collections.Generic;

namespace DbcParserLib.Model;

public class EnvironmentVariable
{
    public EnvDataType Type { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public string Unit { get; internal set; } = string.Empty;
    public string Comment { get; internal set; } = string.Empty;
    public EnvAccessibility Access { get; internal set; }
    public IReadOnlyDictionary<int, string> ValueTableMap { get; internal set; } = new Dictionary<int, string>();
    public IReadOnlyDictionary<string, CustomProperty> CustomProperties => m_customProperties;
    internal readonly Dictionary<string, CustomProperty> m_customProperties = new ();
    public NumericEnvironmentVariable<int>? IntegerEnvironmentVariable { get; internal set; }
    public NumericEnvironmentVariable<double>? FloatEnvironmentVariable { get; internal set; }
    public DataEnvironmentVariable? DataEnvironmentVariable { get; internal set; }
}

public class NumericEnvironmentVariable<T>
{
    public T Minimum { get; }
    public T Maximum { get; }
    public T Default { get; }
    
    public NumericEnvironmentVariable(T minimum, T maximum, T defaultValue)
    {
        Minimum = minimum;
        Maximum = maximum;
        Default = defaultValue;
    }
}

public class DataEnvironmentVariable
{
    public uint Length { get; internal set; }
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