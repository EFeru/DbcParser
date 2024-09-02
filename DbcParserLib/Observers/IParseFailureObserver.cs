namespace DbcParserLib.Observers
{
    public interface IParseFailureObserver
    {
        int CurrentLine {get; set;}

        void DuplicatedMessage(uint messageId);
        void DuplicatedNode(string nodeName);
        void DuplicatedSignalInMessage(uint messageId, string signalName);
        void DuplicatedValueTableName(string tableName);
        void DuplicatedEnvironmentVariableName(string variableName);
        void DuplicatedProperty(string propertyName);
        void DuplicatedPropertyInNode(string propertyName, string nodeName);
        void DuplicatedPropertyInEnvironmentVariable(string propertyName, string environmentVariableName);
        void DuplicatedGlobalProperty(string propertyName);
        void DuplicatedPropertyInMessage(string propertyName, uint messageId);
        void DuplicatedPropertyInSignal(string propertyName, string signalName);
        void DuplicatedEnvironmentVariableInNode(string environmentVariableName, string nodeName);
        void CommentSyntaxError();
        void EnvironmentDataVariableSyntaxError();
        void MessageSyntaxError();
        void EnvironmentVariableSyntaxError();
        void NodeSyntaxError();
        void PropertyDefinitionSyntaxError();
        void PropertySyntaxError();
        void PropertyDefaultSyntaxError();
        void SignalSyntaxError();
        void SignalValueTypeSyntaxError();
        void ValueTableDefinitionSyntaxError();
        void ValueTableSyntaxError();
        void SignalNameNotFound(uint messageId, string signalName);
        void NodeNameNotFound(string nodeName);
        void MessageIdNotFound(uint messageId);
        void EnvironmentVariableNameNotFound(string variableName);
        void PropertyNameNotFound(string propertyName);
        void TableMapNameNotFound(string tableName);
        void PropertyValueOutOfBound(string propertyName, string value);
        void PropertyValueOutOfIndex(string propertyName, string index);
        void UnknownLine();
        void NoMessageFound();
        void Clear();
    }
}