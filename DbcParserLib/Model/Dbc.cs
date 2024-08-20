using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DbcParserLib.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace DbcParserLib.Model;

public class Dbc
{
    public IEnumerable<Node> Nodes { get; }
    public IReadOnlyDictionary<uint, Message> Messages { get; }
    public IEnumerable<EnvironmentVariable> EnvironmentVariables { get; }

    public Dbc(IEnumerable<Node> nodes, Dictionary<uint, Message> messages, IEnumerable<EnvironmentVariable> environmentVariables)
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