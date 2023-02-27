using System;
using System.Collections;
using System.Collections.Generic;

namespace DbcParserLib.Model
{
    public class Node
    {
        public string Name { get; }
        public string Comment { get; }
        public IReadOnlyDictionary<string, CustomProperty> CustomProperties;

        internal Node(EditableNode node)
        {
            Name = node.Name;
            Comment = node.Comment;
            CustomProperties = node.CustomProperties;
        }
    }

    internal class EditableNode
    {
        public string Name;
        public string Comment;
        public Dictionary<string, CustomProperty> CustomProperties = new Dictionary<string, CustomProperty>();

        public Node CreateNode()
        {
            return new Node(this);
        }
    }
}