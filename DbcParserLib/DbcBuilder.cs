using System.Collections.Generic;
using System.Linq;
using DbcParserLib.Model;
using DbcParserLib.Observers;

namespace DbcParserLib;

internal class ValuesTable
{
    public IReadOnlyDictionary<int, string> ValueTableMap { get; set; }
}

internal class DbcBuilder : IDbcBuilder
{
    private readonly IParseFailureObserver m_observer;

    private readonly ISet<Node> m_nodes = new HashSet<Node>(new NodeEqualityComparer());
    private readonly Dictionary<uint, Message> m_messages = new Dictionary<uint, Message>();
    private readonly IDictionary<uint, Dictionary<string, Signal>> m_signals = new Dictionary<uint, Dictionary<string, Signal>>();
    private readonly IDictionary<string, EnvironmentVariable> m_environmentVariables = new Dictionary<string, EnvironmentVariable>();

    private readonly IDictionary<string, ValuesTable> m_namedTablesMap = new Dictionary<string, ValuesTable>();

    private readonly IDictionary<CustomPropertyObjectType, IDictionary<string, CustomProperty>> m_customProperties =
        new Dictionary<CustomPropertyObjectType, IDictionary<string, CustomProperty>>()
        {
            { CustomPropertyObjectType.Node, new Dictionary<string, CustomProperty>() },
            { CustomPropertyObjectType.Message, new Dictionary<string, CustomProperty>() },
            { CustomPropertyObjectType.Signal, new Dictionary<string, CustomProperty>() },
        };

    private Message m_currentMessage;

    public DbcBuilder(IParseFailureObserver observer)
    {
        m_observer = observer;
    }

    public void AddNode(Node node)
    {
        if (m_nodes.Contains(node))
        {
            m_observer.DuplicatedNode(node.Name);
        }
        else
        {
            m_nodes.Add(node);
        }
    }

    public void AddMessage(Message message)
    {
        if (m_messages.TryGetValue(message.ID, out var msg))
        {
            m_currentMessage = msg;
            m_observer.DuplicatedMessage(message.ID);
        }
        else
        {
            m_messages[message.ID] = message;
            m_currentMessage = message;
            m_signals[message.ID] = new Dictionary<string, Signal>();
        }
    }

    public void AddSignal(Signal signal)
    {
        if (m_currentMessage != null)
        {
            signal.MessageID = m_currentMessage.ID;
            if (m_signals[m_currentMessage.ID].TryGetValue(signal.Name, out _))
            {
                m_observer.DuplicatedSignalInMessage(m_currentMessage.ID, signal.Name);
            }
            else
            {
                m_signals[m_currentMessage.ID][signal.Name] = signal;
            }
        }
        else
        {
            m_observer.NoMessageFound();
        }
    }

    public void AddCustomProperty(CustomPropertyObjectType objectType, CustomProperty customProperty)
    {
        if (m_customProperties[objectType].TryGetValue(customProperty.Name, out _))
        {
            m_observer.DuplicatedProperty(customProperty.Name);
        }
        else
        {
            m_customProperties[objectType][customProperty.Name] = customProperty;
        }
    }

    public void AddCustomPropertyDefaultValue(string propertyName, string value, bool isNumeric)
    {
        var found = false;
        foreach (var objectType in m_customProperties.Keys)
        {
            if (m_customProperties[objectType].TryGetValue(propertyName, out var customProperty))
            {
                customProperty.SetCustomPropertyDefaultValue(value, isNumeric);
                found = true;
            }
        }

        if (!found)
        {
            m_observer.PropertyNameNotFound(propertyName);
        }
    }

    public void AddNodeCustomProperty(string propertyName, string nodeName, string value, bool isNumeric)
    {
        if (m_customProperties[CustomPropertyObjectType.Node].TryGetValue(propertyName, out var customProperty))
        {
            var node = m_nodes.FirstOrDefault(n => n.Name.Equals(nodeName));
            if (node != null)
            {
                var clonedProperty = customProperty.Clone();
                if (!clonedProperty.SetCustomPropertyValue(value, isNumeric))
                {
                    return;
                }
                if (node.customProperties.TryGetValue(propertyName, out _))
                {
                    m_observer.DuplicatedPropertyInNode(propertyName, node.Name);
                }
                else
                {
                    node.customProperties[propertyName] = clonedProperty;
                }
            }
            else
            {
                m_observer.NodeNameNotFound(nodeName);
            }
        }
        else
        {
            m_observer.PropertyNameNotFound(propertyName);
        }
    }

    public void AddEnvironmentVariableCustomProperty(string propertyName, string variableName, string value, bool isNumeric)
    {
        if (m_customProperties[CustomPropertyObjectType.Environment].TryGetValue(propertyName, out var customProperty))
        {
            if (m_environmentVariables.TryGetValue(variableName, out var envVariable))
            {
                var clonedProperty = customProperty.Clone();
                if (!clonedProperty.SetCustomPropertyValue(value, isNumeric))
                {
                    return;
                }

                if (envVariable.customProperties.TryGetValue(propertyName, out _))
                {
                    m_observer.DuplicatedPropertyInEnvironmentVariable(propertyName, envVariable.Name);
                }
                else
                {
                    envVariable.customProperties[propertyName] = clonedProperty;
                }
            }
            else
            {
                m_observer.EnvironmentVariableNameNotFound(variableName);
            }
        }
        else
        {
            m_observer.PropertyNameNotFound(propertyName);
        }
    }

    public void AddMessageCustomProperty(string propertyName, uint messageId, string value, bool isNumeric)
    {
        if (m_customProperties[CustomPropertyObjectType.Message].TryGetValue(propertyName, out var customProperty))
        {
            if (m_messages.TryGetValue(messageId, out var message))
            {
                var clonedProperty = customProperty.Clone();
                if (!clonedProperty.SetCustomPropertyValue(value, isNumeric))
                {
                    return;
                }

                if (message.customProperties.TryGetValue(propertyName, out _))
                {
                    m_observer.DuplicatedPropertyInMessage(propertyName, message.ID);
                }
                else
                {
                    message.customProperties[propertyName] = clonedProperty;
                }
            }
            else
            {
                m_observer.MessageIdNotFound(messageId);
            }
        }
        else
        {
            m_observer.PropertyNameNotFound(propertyName);
        }
    }

    public void AddSignalCustomProperty(string propertyName, uint messageId, string signalName, string value, bool isNumeric)
    {
        if (m_customProperties[CustomPropertyObjectType.Signal].TryGetValue(propertyName, out var customProperty))
        {
            if (TryGetValueMessageSignal(messageId, signalName, out var signal))
            {
                var clonedProperty = customProperty.Clone();
                if (!clonedProperty.SetCustomPropertyValue(value, isNumeric))
                {
                    return;
                }

                if (signal.customProperties.TryGetValue(propertyName, out _))
                {
                    m_observer.DuplicatedPropertyInSignal(propertyName, signal.Name);
                }
                else
                {
                    signal.customProperties[propertyName] = clonedProperty;
                }
            }
            else
            {
                m_observer.SignalNameNotFound(messageId, signalName);
            }
        }
        else
        {
            m_observer.PropertyNameNotFound(propertyName);
        }
    }

    public void AddSignalComment(uint messageId, string signalName, string comment)
    {
        if (TryGetValueMessageSignal(messageId, signalName, out var signal))
        {
            signal.Comment = comment;
        }
        else
        {
            m_observer.SignalNameNotFound(messageId, signalName);
        }
    }

    public void AddSignalValueType(uint messageId, string signalName, DbcValueType valueType)
    {
        if (TryGetValueMessageSignal(messageId, signalName, out var signal))
        {
            signal.ValueType = valueType;
        }
        else
        {
            m_observer.SignalNameNotFound(messageId, signalName);
        }
    }

    public void AddSignalExtendedMultiplexingInfo(uint messageId, string signalName, string multiplexorName, List<MultiplexorRange> multiplexorRanges)
    {
        if (TryGetValueMessageSignal(messageId, signalName, out var signal))
        {
            var extendedMultiplex = new ExtendedMultiplex
            {
                MultiplexorSignal = multiplexorName,
                MultiplexorRanges = multiplexorRanges
            };

            signal.extendedMultiplex = extendedMultiplex;
        }
        else
        {
            m_observer.SignalNameNotFound(messageId, signalName);
        }
    }

    public void AddNodeComment(string nodeName, string comment)
    {
        var node = m_nodes.FirstOrDefault(n => n.Name.Equals(nodeName));
        if (node != null)
        {
            node.Comment = comment;
        }
        else
        {
            m_observer.NodeNameNotFound(nodeName);
        }
    }

    public void AddMessageComment(uint messageId, string comment)
    {
        if (m_messages.TryGetValue(messageId, out var message))
        {
            message.Comment = comment;
        }
        else
        {
            m_observer.MessageIdNotFound(messageId);
        }
    }

    public void AddEnvironmentVariableComment(string variableName, string comment)
    {
        if (m_environmentVariables.TryGetValue(variableName, out var envVariable))
        {
            envVariable.Comment = comment;
        }
        else
        {
            m_observer.EnvironmentVariableNameNotFound(variableName);
        }
    }

    public void AddEnvironmentVariable(string variableName, EnvironmentVariable environmentVariable)
    {
        if (m_environmentVariables.TryGetValue(variableName, out _))
        {
            m_observer.DuplicatedEnvironmentVariableName(variableName);
        }
        else
        {
            m_environmentVariables[variableName] = environmentVariable;
        }
    }

    public void AddEnvironmentDataVariable(string variableName, uint dataSize)
    {
        if (m_environmentVariables.TryGetValue(variableName, out var envVariable))
        {
            envVariable.Type = EnvDataType.Data;
            envVariable.DataEnvironmentVariable = new DataEnvironmentVariable()
            {
                Length = dataSize
            };
        }
        else
        {
            m_observer.EnvironmentVariableNameNotFound(variableName);
        }
    }

    public void AddNodeEnvironmentVariable(string nodeName, string variableName)
    {
        var node = m_nodes.FirstOrDefault(n => n.Name.Equals(nodeName));
        if (node != null)
        {
            if (node.environmentVariables.TryGetValue(variableName, out _))
            {
                m_observer.DuplicatedEnvironmentVariableInNode(variableName, node.Name);
            }
            else
            {
                node.environmentVariables[variableName] = m_environmentVariables[variableName];
            }
        }
        else
        {
            m_observer.NodeNameNotFound(nodeName);
        }
    }

    public void AddNamedValueTable(string name, IReadOnlyDictionary<int, string> dictValues)
    {
        if (m_namedTablesMap.TryGetValue(name, out _))
        {
            m_observer.DuplicatedValueTableName(name);
        }
        else
        {
            m_namedTablesMap[name] = new ValuesTable()
            {
                ValueTableMap = dictValues,
            };
        }
    }

    public void LinkTableValuesToSignal(uint messageId, string signalName, IReadOnlyDictionary<int, string> dictValues)
    {
        if (TryGetValueMessageSignal(messageId, signalName, out var signal))
        {
            signal.ValueTableMap = dictValues;
        }
        else
        {
            m_observer.SignalNameNotFound(messageId, signalName);
        }
    }

    public void LinkTableValuesToEnvironmentVariable(string variableName, IReadOnlyDictionary<int, string> dictValues)
    {
        if (m_environmentVariables.TryGetValue(variableName, out var envVariable))
        {
            envVariable.ValueTableMap = dictValues;
        }
        else
        {
            m_observer.EnvironmentVariableNameNotFound(variableName);
        }
    }

    public void LinkNamedTableToSignal(uint messageId, string signalName, string tableName)
    {
        if (m_namedTablesMap.TryGetValue(tableName, out var valuesTable))
        {
            LinkTableValuesToSignal(messageId, signalName, valuesTable.ValueTableMap);
        }
        else
        {
            m_observer.TableMapNameNotFound(tableName);
        }
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

    private void FillNodesNotSetCustomPropertyWithDefault()
    {
        var nodeCustomProperties = m_customProperties[CustomPropertyObjectType.Node];
        foreach (var customProperty in nodeCustomProperties)
        {
            // See comment in "AddSignalCustomProperty"
            customProperty.Value.SetDefaultIfNotSet(); 
            foreach (var node in m_nodes)
            {
                if (!node.customProperties.TryGetValue(customProperty.Key, out _))
                {
                    node.customProperties[customProperty.Key] = customProperty.Value;
                }
            }
        }
    }

    private void FillMessagesNotSetCustomPropertyWithDefault()
    {
        var messageCustomProperties = m_customProperties[CustomPropertyObjectType.Message];
        foreach (var customProperty in messageCustomProperties)
        {
            // See comment in "AddSignalCustomProperty"
            customProperty.Value.SetDefaultIfNotSet(); 
            foreach (var message in m_messages.Values)
            {
                FillSignalsNotSetCustomPropertyWithDefault(message.ID);
                if (!message.customProperties.TryGetValue(customProperty.Key, out _))
                {
                    message.customProperties[customProperty.Key] = customProperty.Value;
                }
            }
        }
    }

    private void FillSignalsNotSetCustomPropertyWithDefault(uint messageId)
    {
        var signalCustomProperties = m_customProperties[CustomPropertyObjectType.Signal];
        foreach (var customProperty in signalCustomProperties)
        {
            // If a value gets set via "AddSignalCustomProperty" it always only gets set in a copy assigned to a specific signal.
            // Because of that by definition the default value from the definition doesn't get set in the originally created customProperty
            // It is enough to set it one now as all other signals will just get a ref to this base property (to not create unnecessary copies)
            customProperty.Value.SetDefaultIfNotSet(); 
            foreach (var signal in m_signals[messageId].Values)
            {
                if (!signal.customProperties.TryGetValue(customProperty.Key, out _))
                {
                    signal.customProperties[customProperty.Key] = customProperty.Value;
                }
            }
        }
    }

    public Dbc Build()
    {
        FillNodesNotSetCustomPropertyWithDefault();
        FillMessagesNotSetCustomPropertyWithDefault();

        foreach (var message in m_messages)
        {
            if (m_signals.TryGetValue(message.Key, out var signals) && signals != null)
            {
                message.Value.signals = signals;
            }
        }

        return new Dbc(m_nodes.ToList(), m_messages, m_environmentVariables.Values.ToList());
    }
}

internal class NodeEqualityComparer : IEqualityComparer<Node>
{
    public bool Equals(Node b1, Node b2)
    {
        if (b2 == null && b1 == null)
        {
            return true;
        }
        else if (b1 == null || b2 == null)
        {
            return false;
        }
        else if (b1.Name == b2.Name)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public int GetHashCode(Node bx)
    {
        return bx.Name.GetHashCode();
    }
}