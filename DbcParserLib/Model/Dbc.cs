using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DbcParserLib.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace DbcParserLib.Model;
public class Dbc
{
    public IReadOnlyCollection<Node> Nodes { get; }
    public IReadOnlyDictionary<uint, Message> Messages { get; }
    public IReadOnlyCollection<EnvironmentVariable> EnvironmentVariables { get; }

    public Dbc(List<Node> nodes, Dictionary<uint, Message> messages, List<EnvironmentVariable> environmentVariables)
    {
        Nodes = nodes;
        Messages = messages;
        EnvironmentVariables = environmentVariables;
        FinishUp();
    }

    private void FinishUp()
    {
        foreach (var message in Messages.Values)
        {
            message.FinishUp();
        }
    }
}