using System;
using System.Collections;
using System.Collections.Generic;

namespace DbcParserLib.Model
{
    internal class ImmutableNode
    {
        public string Name { get; }
        public string Comment { get; }
        public IReadOnlyDictionary<string, CustomProperty> CustomProperties { get; }
        public IReadOnlyDictionary<string, ImmutableEnvironmentVariable> EnvironmentVariables { get; }

        internal ImmutableNode(Node node, IReadOnlyDictionary<string, ImmutableEnvironmentVariable> environmentVariables)
        {
            Name = node.Name;
            Comment = node.Comment;
            //TODO: remove explicit cast (CustomProperty in Node class should be Dictionary instead IDictionary)
            CustomProperties = (IReadOnlyDictionary<string, CustomProperty>)node.CustomProperties;
            EnvironmentVariables = environmentVariables;
        }
    }

    public class Node
    {
        public string Name;
        public string Comment;
        public IDictionary<string, CustomProperty> CustomProperties = new Dictionary<string, CustomProperty>();
        public readonly IDictionary<string, EnvironmentVariable> EnvironmentVariables = new Dictionary<string, EnvironmentVariable>();

        internal ImmutableNode CreateNode()
        {
            var environmentVariables = new Dictionary<string, ImmutableEnvironmentVariable>();
            foreach (var environmentVariable in EnvironmentVariables)
            {
                environmentVariables.Add(environmentVariable.Key, environmentVariable.Value.CreateEnvironmentVariable());
            }
            return new ImmutableNode(this, environmentVariables);
        }
    }
}