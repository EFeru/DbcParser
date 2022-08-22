using System.Collections.Generic;
using DbcParserLib.Model;

namespace DbcParserLib
{
    public class Dbc
    {
        public IEnumerable<Node> Nodes {get;}
        public IEnumerable<Message> Messages {get;}

        public Dbc(IEnumerable<Node> nodes, IEnumerable<Message> messages)
        {
            Nodes = nodes;
            Messages = messages;
        }
    }
}