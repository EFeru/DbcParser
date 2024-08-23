using System.Collections.Generic;

namespace DbcParserLib.Model;

public class Node
{
    public string Name { get; internal set; } = string.Empty;
    public string Comment { get; internal set;  } = string.Empty;
    
    public IReadOnlyDictionary<string, CustomProperty> CustomProperties => customProperties;
    internal readonly Dictionary<string, CustomProperty> customProperties = new ();

    public IReadOnlyDictionary<string, EnvironmentVariable> EnvironmentVariables => environmentVariables;
    internal readonly Dictionary<string, EnvironmentVariable> environmentVariables = new ();
}