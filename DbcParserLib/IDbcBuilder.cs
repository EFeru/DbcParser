using DbcParserLib.Model;
using System.Collections.Generic;

namespace DbcParserLib
{
    internal interface IDbcBuilder
    {
        void AddMessage(Message message);
        void AddMessageComment(uint messageId, string comment);
        void AddNamedValueTable(string name, IReadOnlyDictionary<int, string> dictValues);
        void AddNode(Node node);
        void AddNodeComment(string nodeName, string comment);
        void AddSignal(Signal signal);
        void AddSignalComment(uint messageId, string signalName, string comment);
        void AddSignalValueType(uint messageId, string signalName, DbcValueType valueType);
        void AddSignalExtendedMultiplexingInfo(uint messageId, string signalName, ParsingExtendedMultiplexing extendedMultiplexing);
        void LinkNamedTableToSignal(uint messageId, string signalName, string tableName);
        void LinkTableValuesToSignal(uint messageId, string signalName, IReadOnlyDictionary<int, string> dictValues);
        void LinkTableValuesToEnvironmentVariable(string variableName, IReadOnlyDictionary<int, string> dictValues);
        void AddCustomProperty(CustomPropertyObjectType objectType, CustomPropertyDefinition customProperty);
        void AddCustomPropertyDefaultValue(string propertyName, string value, bool isNumeric);
        void AddNodeCustomProperty(string propertyName, string nodeName, string value, bool isNumeric);
        void AddEnvironmentVariableCustomProperty(string propertyName, string variableName, string value, bool isNumeric);
        void AddMessageCustomProperty(string propertyName, uint messageId, string value, bool isNumeric);
        void AddSignalCustomProperty(string propertyName, uint messageId, string signalName, string value, bool isNumeric);
        void AddEnvironmentVariable(string variableName, EnvironmentVariable environmentVariable);
        void AddEnvironmentVariableComment(string variableName, string comment);
        void AddEnvironmentDataVariable(string variableName, uint dataSize);
        void AddNodeEnvironmentVariable(string nodeName, string variableName);
    }
}