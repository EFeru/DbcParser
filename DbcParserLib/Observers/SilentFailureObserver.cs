namespace DbcParserLib.Observers
{
    public class SilentFailureObserver : IParseFailureObserver
    {
        public int CurrentLine {get; set;}

        public void DuplicatedMessage(uint messageId)
        {
        }

        public void DuplicatedNode(string nodeName)
        {
        }

        public void DuplicatedSignalInMessage(uint messageId, string signalName)
        {
        }

        public void DuplicatedValueTableName(string tableName)
        {
        }

        public void DuplicatedEnvironmentVariableName(string variableName)
        {
        }

        public void DuplicatedProperty(string propertyName)
        {
        }

        public void DuplicatedPropertyInNode(string propertyName, string nodeName)
        {
        }

        public void DuplicatedPropertyInEnvironmentVariable(string propertyName, string environmentVariableName)
        {
        }

        public void DuplicatedGlobalProperty(string propertyName)
        {
        }

        public void DuplicatedPropertyInMessage(string propertyName, uint messageId)
        {
        }

        public void DuplicatedPropertyInSignal(string propertyName, string signalName)
        {
        }

        public void DuplicatedEnvironmentVariableInNode(string environmentVariableName, string nodeName)
        {
        }

        public void CommentSyntaxError()
        {
        }

        public void EnvironmentDataVariableSyntaxError()
        {
        }

        public void MessageSyntaxError()
        {
        }

        public void EnvironmentVariableSyntaxError()
        {
        }

        public void NodeSyntaxError()
        {
        }

        public void PropertyDefinitionSyntaxError()
        {
        }

        public void PropertySyntaxError()
        {
        }

        public void PropertyDefaultSyntaxError()
        {
        }

        public void SignalSyntaxError()
        {
        }

        public void SignalValueTypeSyntaxError()
        {
        }

        public void ValueTableDefinitionSyntaxError()
        {
        }

        public void ValueTableSyntaxError()
        {
        }

        public void SignalNameNotFound(uint messageId, string signalName)
        {
        }

        public void NodeNameNotFound(string nodeName)
        {
        }

        public void MessageIdNotFound(uint messageId)
        {
        }

        public void EnvironmentVariableNameNotFound(string variableName)
        {
        }

        public void PropertyNameNotFound(string propertyName)
        {
        }

        public void TableMapNameNotFound(string tableName)
        {
        }

        public void PropertyValueOutOfBound(string propertyName, string value)
        {
        }

        public void PropertyValueOutOfIndex(string propertyName, string index)
        {
        }

        public void ExtraMessageTransmittersSyntaxError()
        {
        }

        public void ExtraMessageTransmittersDuplicate(uint messageId, string duplicateTransmitter)
        {
        }

        public void SignalExtendedMultiplexingSyntaxError()
        {
        }

        public void UnknownLine()
        {
        }

        public void NoMessageFound()
        {
        }

        public void Clear()
        {
        }
    }
}