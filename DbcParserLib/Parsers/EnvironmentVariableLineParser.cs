using DbcParserLib.Model;
using DbcParserLib.Observers;
using System.Text.RegularExpressions;

namespace DbcParserLib.Parsers
{
    internal class EnvironmentVariableLineParser : ILineParser
    {
        private const string EnvironmentVariableLineStarter = "EV_ ";
        private const string EnvironmentVariableParsingRegex = @"EV_\s+([a-zA-Z_][\w]*)\s*:\s+([012])\s+\[([\d\+\-eE.]+)\|([\d\+\-eE.]+)\]\s+""([^""]*)""\s+([\d\+\-eE.]+)\s+(\d+)\s+DUMMY_NODE_VECTOR(800){0,1}([0123])\s+((?:[a-zA-Z_][\w]*)(?:,[a-zA-Z_][\w]*)*)\s*;";

        private readonly IParseFailureObserver m_observer;

        public EnvironmentVariableLineParser(IParseFailureObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var cleanLine = line.Trim().ReplaceNewlinesWithSpace();

            if (cleanLine.StartsWith(EnvironmentVariableLineStarter) == false)
                return false;

            var match = Regex.Match(cleanLine, EnvironmentVariableParsingRegex);
            if (match.Success)
            {
                var environmentVariable = new EnvironmentVariable()
                {
                    Name = match.Groups[1].Value,
                    Unit = match.Groups[5].Value,
                };

                switch (uint.Parse(match.Groups[9].Value))
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

                if (match.Groups[8].Value == "800")
                {
                    environmentVariable.Type = EnvDataType.String;
                }
                else
                {
                    switch (uint.Parse(match.Groups[2].Value))
                    {
                        case 0:
                            environmentVariable.Type = EnvDataType.Integer;
                            environmentVariable.IntegerEnvironmentVariable = new NumericEnvironmentVariable<int>()
                            {
                                Minimum = int.Parse(match.Groups[3].Value),
                                Maximum = int.Parse(match.Groups[4].Value),
                                Default = int.Parse(match.Groups[6].Value)
                            };
                            break;
                        case 1:
                            environmentVariable.Type = EnvDataType.Float;
                            environmentVariable.FloatEnvironmentVariable = new NumericEnvironmentVariable<double>()
                            {
                                Minimum = double.Parse(match.Groups[3].Value),
                                Maximum = double.Parse(match.Groups[4].Value),
                                Default = double.Parse(match.Groups[6].Value)
                            };
                            break;
                        case 2:
                            environmentVariable.Type = EnvDataType.String;
                            break;
                    }
                }

                builder.AddEnvironmentVariable(match.Groups[1].Value, environmentVariable);

                var nodes = match.Groups[10].Value.Split(',');
                foreach (var node in nodes)
                {
                    builder.AddNodeEnvironmentVariable(node, match.Groups[1].Value);
                }
            }
            else
                m_observer.EnvironmentVariableSyntaxError();
            
            return true;
        }
    }
}