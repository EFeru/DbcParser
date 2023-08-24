using System.Collections.Generic;

namespace DbcParserLib.Observers
{
    public class SimpleFailureObserver : IParseFailureObserver

    {
        private readonly IList<string> m_errors = new List<string>();

        public int CurrentLine {get; set;}

        private void AddError(string error)
        {
            m_errors.Add($"{error} at line {CurrentLine}");
        }

        public void DuplicateMessage(uint messageId)
        {
            AddError($"Duplicated message (ID {messageId})");
        }

        public void DuplicateNode(string nodeName)
        {
            AddError($"Duplicated node '{nodeName}'");
        }

        public void DuplicateSignalInMessage(uint messageId, string signalName)
        {
            AddError($"Duplicated signal '{signalName}' in message (ID {messageId})");
        }

        public void DuplicateValueTableName(string tableName)
        {
            AddError($"Duplicated value table '{tableName}'");
        }

        public void DuplicateEnvironmentVariableName(string variableName)
        {
            AddError($"Duplicated environment variable '{variableName}'");
        }

        public void DuplicateCustomProperty(string propertyName)
        {
            AddError($"Duplicated custom property '{propertyName}'");
        }

        public void DuplicateCustomPropertyInNode(string propertyName, string nodeName)
        {
            AddError($"Duplicated custom property '{propertyName}' in node '{nodeName}'");
        }

        public void DuplicateCustomPropertyInEnvironmentVariable(string propertyName, string environmentVariableName)
        {
            AddError($"Duplicated custom property '{propertyName}' in environment variable '{environmentVariableName}'");
        }

        public void DuplicateCustomPropertyInMessage(string propertyName, uint messageId)
        {
            AddError($"Duplicated custom property '{propertyName}' in message (ID {messageId})");
        }

        public void DuplicateCustomPropertyInSignal(string propertyName, string signalName)
        {
            AddError($"Duplicated custom property '{propertyName}' in signal '{signalName}'");
        }

        public void DuplicateEnvironmentVariableInNode(string environmentVariableName, string nodeName)
        {
            AddError($"Duplicated environment variable '{environmentVariableName}' in node '{nodeName}'");
        }

        public void CommentSyntaxError()
        {
            AddError("[CM_] Comment syntax error");
        }

        public void EnvironmentDataVariableSyntaxError()
        {
            AddError("[ENVVAR_DATA_] Environment data variable syntax error");
        }

        public void MessageSyntaxError()
        {
            AddError("[BO_] Message syntax error");
        }

        public void EnvironmentVariableSyntaxError()
        {
            AddError("[EV_] Environment variable syntax error");
        }

        public void NodeSyntaxError()
        {
            AddError("[BU_] Node syntax error");
        }

        public void PropertyDefinitionSyntaxError()
        {
            AddError("[BA_DEF_] Property definition syntax error");
        }

        public void PropertySyntaxError()
        {
            AddError("[BA_] Property syntax error");
        }

        public void PropertyDefaultSyntaxError()
        {
            AddError("[BA_DEF_DEF_] Property default value syntax error");
        }

        public void SignalSyntaxError()
        {
            AddError("[SG_] Signal syntax error");
        }

        public void SignalValueTypeSyntaxError()
        {
            AddError("[SIG_VALTYPE_] Signal value type syntax error");
        }

        public void ValueTableDefinitionSyntaxError()
        {
            AddError("[VAL_TABLE_] Value table definition syntax error");
        }

        public void ValueTableSyntaxError()
        {
            AddError("[VAL_] Value table syntax error");
        }

        public void SignalNameNotFound(uint messageId, string signalName)
        {
            AddError($"Signal '{signalName}' in message (ID {messageId}) not found");
        }

        public void NodeNameNotFound(string nodeName)
        {
            AddError($"Node '{nodeName}' not found");
        }

        public void MessageIdNotFound(uint messageId)
        {
            AddError($"Message (ID {messageId}) not found");
        }

        public void EnvironmentVariableNameNotFound(string variableName)
        {
            AddError($"Environment variable '{variableName}' not found");
        }

        public void CustomPropertyNameNotFound(string propertyName)
        {
            AddError($"Custom property '{propertyName}' not found");
        }

        public void TableMapNameNotFound(string tableName)
        {
            AddError($"Table map '{tableName}' not found");
        }

        public void UnknownLine()
        {
            AddError("Unknown syntax");
        }

        public void NoMessageFound()
        {
            AddError("No message has been defined yet");
        }

        public void Clear()
        {
            CurrentLine = 0;
            m_errors.Clear();
        }

        public IList<string> GetErrorList()
        {
            return m_errors;
        }
    }
}