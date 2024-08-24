using System.Collections.Generic;

namespace DbcParserLib.Model;

public class Node
{
    public string Name { get; internal set; } = string.Empty;
    public string Comment { get; internal set;  } = string.Empty;
    
    public IReadOnlyDictionary<string, CustomProperty> CustomProperties => m_customProperties;
    internal readonly Dictionary<string, CustomProperty> m_customProperties = new ();

    public IReadOnlyDictionary<string, EnvironmentVariable> EnvironmentVariables => m_environmentVariables;
    internal readonly Dictionary<string, EnvironmentVariable> m_environmentVariables = new ();
}