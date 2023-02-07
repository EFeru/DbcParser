using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DbcParserLib.Model;

namespace DbcParserLib
{
    public class DbcBuilder : IDbcBuilder
    {
        private readonly ISet<Node> m_nodes = new HashSet<Node>(new NodeEqualityComparer());
        private readonly IDictionary<string, string> m_namedTables = new Dictionary<string, string>();
        private readonly IDictionary<uint, Message> m_messages = new Dictionary<uint, Message>();
        private readonly IDictionary<uint, IDictionary<string, Signal>> m_signals = new Dictionary<uint, IDictionary<string, Signal>>();

        private Message m_currentMessage;

        public void AddNode(Node node)
        {
            m_nodes.Add(node);
        }

        public void AddMessage(Message message)
        {
            m_messages[message.ID] = message;
            m_currentMessage = message;
            m_signals[message.ID] = new Dictionary<string, Signal>();
        }

        public void AddSignal(Signal signal)
        {
            if (m_currentMessage != null)
            {
                signal.ID = m_currentMessage.ID;
                m_signals[m_currentMessage.ID][signal.Name] = signal;
            }
        }

        public void AddSignalComment(uint messageId, string signalName, string comment)
        {
            if (TryGetValueMessageSignal(messageId, signalName, out var signal))
            {
                signal.Comment = comment;
            }
        }

        public void AddSignalInitialValue(uint messageId, string signalName, double initialValue)
        {
            IsExtID(ref messageId);
            if (TryGetValueMessageSignal(messageId, signalName, out var signal))
            {
                signal.InitialValue = initialValue * signal.Factor + signal.Offset;
            }
        }

        public void AddSignalValueType(uint messageId, string signalName, DbcValueType valueType)
        {
            if (TryGetValueMessageSignal(messageId, signalName, out var signal))
            {
                signal.ValueType = valueType;
            }
        }

        public void AddNodeComment(string nodeName, string comment)
        {
            var node = m_nodes.FirstOrDefault(n => n.Name.Equals(nodeName));
            if (node != null)
            {
                node.Comment = comment;
            }
        }

        public void AddMessageComment(uint messageId, string comment)
        {
            if (m_messages.TryGetValue(messageId, out var message))
            {
                message.Comment = comment;
            }
        }

        public void AddMessageCycleTime(uint messageId, int cycleTime)
        {
            IsExtID(ref messageId);
            if (m_messages.TryGetValue(messageId, out var message))
            {
                message.CycleTime = cycleTime;
            }
        }

        public void AddNamedValueTable(string name, string values)
        {
            m_namedTables[name] = values;
        }

        public void LinkTableValuesToSignal(uint messageId, string signalName, string values)
        {
            IsExtID(ref messageId);
            if (TryGetValueMessageSignal(messageId, signalName, out var signal))
            {
                signal.ValueTable = values;
            }
        }
        public static bool IsExtID(ref uint id)
        {
            // For extended ID bit 31 is always 1
            if (id >= 0x80000000)
            {
                id -= 0x80000000;
                return true;
            }
            else
                return false;
        }

        private bool TryGetValueMessageSignal(uint messageId, string signalName, out Signal signal)
        {
            if (m_signals.TryGetValue(messageId, out var signals) && signals.TryGetValue(signalName, out signal))
            {
                return true;
            }

            signal = null;
            return false;
        }

        public void LinkNamedTableToSignal(uint messageId, string signalName, string tableName)
        {
            if (m_namedTables.TryGetValue(tableName, out var values))
            {
                LinkTableValuesToSignal(messageId, signalName, values);
            }
        }

        public Dbc Build()
        {
            foreach (var message in m_messages)
            {
                message.Value.Signals.Clear();
                message.Value.Signals.AddRange(m_signals[message.Key].Values);
            }

            return new Dbc(m_nodes.ToArray(), m_messages.Values.ToArray());
        }
    }

    internal class NodeEqualityComparer : IEqualityComparer<Node>
    {
        public bool Equals(Node b1, Node b2)
        {
            if (b2 == null && b1 == null)
                return true;
            else if (b1 == null || b2 == null)
                return false;
            else if(b1.Name == b2.Name)
                return true;
            else
                return false;
        }

        public int GetHashCode(Node bx)
        {
            return bx.Name.GetHashCode();
        }
    }
}