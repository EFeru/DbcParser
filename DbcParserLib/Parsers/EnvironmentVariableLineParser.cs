using DbcParserLib.Model;
using DbcParserLib.Observers;
using System.Text.RegularExpressions;

namespace DbcParserLib.Parsers
{
    internal class EnvironmentVariableLineParser : ILineParser
    {
        private const string NameGroup = "Name";
        private const string VarTypeGroup = "VarType";
        private const string MinGroup = "Min";
        private const string MaxGroup = "Max";
        private const string UnitGroup = "Unit";
        private const string InitialValueGroup = "InitialValue";
        private const string VarId = "VarId";
        private const string StringDataTypeGroup = "StringDataType";
        private const string AccessibilityGroup = "Accessibility2";
        private const string NodeGroup = "Node";
        private const string EnvironmentVariableLineStarter = "EV_ ";

        private readonly string m_environmentVariableParsingRegex = $@"EV_\s+(?<{NameGroup}>[a-zA-Z_][\w]*)\s*:\s+(?<{VarTypeGroup}>[012])\s+\[(?<{MinGroup}>[\d\+\-eE.]+)\|(?<{MaxGroup}>[\d\+\-eE.]+)\]" +
                                                                    $@"\s+""(?<{UnitGroup}>[^""]*)""\s+(?<{InitialValueGroup}>[\d\+\-eE.]+)\s+(?<{VarId}>\d+)\s+DUMMY_NODE_VECTOR(?<{StringDataTypeGroup}>800){{0,1}}" +
                                                                    $@"(?<{AccessibilityGroup}>[0123])\s+(?<{NodeGroup}>(?:[a-zA-Z_][\w]*)(?:,[a-zA-Z_][\w]*)*)\s*;";

        private readonly IParseFailureObserver m_observer;

        public EnvironmentVariableLineParser(IParseFailureObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var cleanLine = line.ReplaceNewlinesWithSpace().Trim();

            if (cleanLine.StartsWith(EnvironmentVariableLineStarter) == false)
                return false;

            var match = Regex.Match(cleanLine, m_environmentVariableParsingRegex);
            if (match.Success)
            {
                var environmentVariable = new EnvironmentVariable()
                {
                    Name = match.Groups[NameGroup].Value,
                    Unit = match.Groups[UnitGroup].Value,
                };

                switch (uint.Parse(match.Groups[AccessibilityGroup].Value))
                {
                    case 0:
                        environmentVariable.Access = EnvAccessibility.Unrestricted;
                        break;
                    case 1:
                        environmentVariable.Access = EnvAccessibility.Read;
                        break;
                    case 2:
                        environmentVariable.Access = EnvAccessibility.Write;
                        break;
                    case 3:
                        environmentVariable.Access = EnvAccessibility.ReadWrite;
                        break;
                }

                if (match.Groups[StringDataTypeGroup].Value == "800")
                {
                    environmentVariable.Type = EnvDataType.String;
                }
                else
                {
                    switch (uint.Parse(match.Groups[VarTypeGroup].Value))
                    {
                        case 0:
                            environmentVariable.Type = EnvDataType.Integer;
                            environmentVariable.IntegerEnvironmentVariable = new NumericEnvironmentVariable<int>()
                            {
                                Minimum = int.Parse(match.Groups[MinGroup].Value),
                                Maximum = int.Parse(match.Groups[MaxGroup].Value),
                                Default = int.Parse(match.Groups[InitialValueGroup].Value)
                            };
                            break;
                        case 1:
                            environmentVariable.Type = EnvDataType.Float;
                            environmentVariable.FloatEnvironmentVariable = new NumericEnvironmentVariable<double>()
                            {
                                Minimum = double.Parse(match.Groups[MinGroup].Value),
                                Maximum = double.Parse(match.Groups[MaxGroup].Value),
                                Default = double.Parse(match.Groups[InitialValueGroup].Value)
                            };
                            break;
                        case 2:
                            environmentVariable.Type = EnvDataType.String;
                            break;
                    }
                }

                builder.AddEnvironmentVariable(match.Groups[NameGroup].Value, environmentVariable);

                var nodes = match.Groups[NodeGroup].Value.Split(',');
                foreach (var node in nodes)
                {
                    builder.AddNodeEnvironmentVariable(node, match.Groups[NameGroup].Value);
                }
            }
            else
                m_observer.EnvironmentVariableSyntaxError();

            return true;
        }
    }
}