namespace DbcParserLib.Observers
{
    public interface IParseFailureObserver
    {
        int CurrentLine {get; set;}

        void DuplicateMessage(uint messageId);
        void DuplicateNode(string nodeName);
        void DuplicateSignalInMessage(uint messageId, string signalName);
        void DuplicateValueTableName(string tableName);
        void DuplicateEnvironmentVariableName(string variableName);
        void DuplicateCustomProperty(string propertyName);
        void DuplicateCustomPropertyInNode(string propertyName, string nodeName);
        void DuplicateCustomPropertyInEnvironmentVariable(string propertyName, string environmentVariableName);
        void DuplicateCustomPropertyInMessage(string propertyName, uint messageId);
        void DuplicateCustomPropertyInSignal(string propertyName, string signalName);
        void DuplicateEnvironmentVariableInNode(string environmentVariableName, string nodeName);
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
        void CustomPropertyNameNotFound(string propertyName);
        void TableMapNameNotFound(string tableName);
        void UnknownLine();
        void NoMessageFound();
        void Clear();
    }
}