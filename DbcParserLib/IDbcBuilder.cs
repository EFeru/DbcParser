using DbcParserLib.Model;
using System.Collections.Generic;

namespace DbcParserLib
{
    internal interface IDbcBuilder
    {
        void AddMessage(EditableMessage message);
        void AddMessageComment(uint messageId, string comment);
        void AddMessageCycleTime(uint messageId, int cycleTime);
        void AddNamedValueTable(string name, IReadOnlyDictionary<int, string> dictValues, string stringValues);
        void AddNode(EditableNode node);
        void AddNodeComment(string nodeName, string comment);
        void AddSignal(EditableSignal signal);
        void AddSignalComment(uint messageId, string signalName, string comment);
        void AddSignalInitialValue(uint messageId, string signalName, double initialValue);
        void AddSignalValueType(uint messageId, string signalName, DbcValueType valueType);
        void LinkNamedTableToSignal(uint messageId, string signalName, string tableName);
        void LinkTableValuesToSignal(uint messageId, string signalName, IReadOnlyDictionary<int, string> dictValues, string stringValues);
        void AddCustomProperty(DbcObjectType objectType, CustomPropertyDefinition customProperty);
        void AddCustomPropertyDefaultValue(string propertyName, string value);
        void AddNodeCustomProperty(string propertyName, string nodeName, string value);
        void AddMessageCustomProperty(string propertyName, uint messageId, string value);
        void AddSignalCustomProperty(string propertyName, uint messageId, string signalName, string value);
    }
}