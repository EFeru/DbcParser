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

        internal ImmutableNode(Node node)
        {
            Name = node.Name;
            Comment = node.Comment;
            //TODO: remove explicit cast (CustomProperty in Node class should be Dictionary instead IDictionary)
            CustomProperties = (IReadOnlyDictionary<string, CustomProperty>)node.CustomProperties;
        }
    }

    public class Node
    {
        public string Name;
        public string Comment;
        public IDictionary<string, CustomProperty> CustomProperties = new Dictionary<string, CustomProperty>();

        internal ImmutableNode CreateNode()
        {
            return new ImmutableNode(this);
        }
    }
}