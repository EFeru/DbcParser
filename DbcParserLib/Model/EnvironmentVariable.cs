using System.Collections.Generic;

namespace DbcParserLib.Model
{
    public class ImmutableEnvironmentVariable
    {
        public EnvDataType Type { get; }
        public string Name { get; }
        public string Unit { get; }
        public string Comment { get; }
        public EnvAccessibility Access { get; }
        public IReadOnlyDictionary<int, string> ValueTableMap { get; }
        public IReadOnlyDictionary<string, CustomProperty> CustomProperties { get; }
        public NumericEnvironmentVariable<int> IntegerEnvironmentVariable { get; }
        public NumericEnvironmentVariable<double> FloatEnvironmentVariable { get; }
        public DataEnvironmentVariable DataEnvironmentVariable { get; }

        internal ImmutableEnvironmentVariable(EnvironmentVariable environmentVariable)
        {
            //TODO: remove explicit cast (CustomProperty in Message class should be Dictionary instead IDictionary)
            CustomProperties = (IReadOnlyDictionary<string, CustomProperty>)environmentVariable.CustomProperties;
            Type = environmentVariable.Type;
            Name = environmentVariable.Name;
            Unit = environmentVariable.Unit;
            Comment = environmentVariable.Comment;
            Access = environmentVariable.Access;
            ValueTableMap = environmentVariable.ValueTableMap;
            IntegerEnvironmentVariable = environmentVariable.IntegerEnvironmentVariable;
            FloatEnvironmentVariable = environmentVariable.FloatEnvironmentVariable;
            DataEnvironmentVariable = environmentVariable.DataEnvironmentVariable;
        }
    }

    public class EnvironmentVariable
    {
        public EnvDataType Type { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public string Comment { get; set; }
        public EnvAccessibility Access { get; set; }
        public IReadOnlyDictionary<int, string> ValueTableMap { get; set; }

        public readonly IDictionary<string, CustomProperty> CustomProperties = new Dictionary<string, CustomProperty>();
        public NumericEnvironmentVariable<int> IntegerEnvironmentVariable { get; set; }
        public NumericEnvironmentVariable<double> FloatEnvironmentVariable { get; set; }
        public DataEnvironmentVariable DataEnvironmentVariable { get; set; }

        internal ImmutableEnvironmentVariable CreateEnvironmentVariable()
        {
            return new ImmutableEnvironmentVariable(this);
        }
    }

    public class NumericEnvironmentVariable<T>
    {
        public T Minimum { get; set; }
        public T Maximum { get; set; }
        public T Default { get; set; }
    }

    public class DataEnvironmentVariable
    {
        public uint Length { get; set; }
    }

    public enum EnvDataType
    {
        Integer, Float, String, Data
    }

    public enum EnvAccessibility
    {
        Unrestricted, Read, Write, ReadWrite
    }
}
