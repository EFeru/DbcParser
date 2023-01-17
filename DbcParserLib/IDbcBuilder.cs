using DbcParserLib.Model;

namespace DbcParserLib
{
    public interface IDbcBuilder
    {
        void AddMessage(Message message);
        void AddMessageComment(uint messageId, string comment);
        void AddMessageCycleTime(uint messageId, int cycleTime);
        void AddNamedValueTable(string name, string values);
        void AddNode(Node node);
        void AddNodeComment(string nodeName, string comment);
        void AddSignal(Signal signal);
        void AddSignalComment(uint messageId, string signalName, string comment);
        void AddSignalInitialValue(uint messageId, string signalName, double initialValue);
        void AddSignalValueType(uint messageId, string signalName, DbcValueType valueType);
        void LinkNamedTableToSignal(uint messageId, string signalName, string tableName);
        void LinkTableValuesToSignal(uint messageId, string signalName, string values);
    }
}