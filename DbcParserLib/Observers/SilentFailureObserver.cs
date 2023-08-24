namespace DbcParserLib.Observers
{
    public class SilentFailureObserver : IParseFailureObserver
    {
        public int CurrentLine {get; set;}

        public void DuplicateMessage(uint messageId)
        {
        }

        public void DuplicateNode(string nodeName)
        {
        }

        public void DuplicateSignalInMessage(uint messageId, string signalName)
        {
        }

        public void DuplicateValueTableName(string tableName)
        {
        }

        public void DuplicateEnvironmentVariableName(string variableName)
        {
        }

        public void DuplicateCustomProperty(string propertyName)
        {
        }

        public void DuplicateCustomPropertyInNode(string propertyName, string nodeName)
        {
        }

        public void DuplicateCustomPropertyInEnvironmentVariable(string propertyName, string environmentVariableName)
        {
        }

        public void DuplicateCustomPropertyInMessage(string propertyName, uint messageId)
        {
        }

        public void DuplicateCustomPropertyInSignal(string propertyName, string signalName)
        {
        }

        public void DuplicateEnvironmentVariableInNode(string environmentVariableName, string nodeName)
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

        public void CustomPropertyNameNotFound(string propertyName)
        {
        }

        public void TableMapNameNotFound(string tableName)
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