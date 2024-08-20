using System.Collections.Generic;

namespace DbcParserLib.Model;

public class Node
{
    public string Name { get; internal set; }
    public string Comment { get; internal set;  }
    
    public IReadOnlyDictionary<string, CustomProperty> CustomProperties => customProperties;
    internal readonly Dictionary<string, CustomProperty> customProperties = new ();

    public IReadOnlyDictionary<string, EnvironmentVariable> EnvironmentVariables => environmentVariables;
    internal readonly Dictionary<string, EnvironmentVariable> environmentVariables = new ();
}