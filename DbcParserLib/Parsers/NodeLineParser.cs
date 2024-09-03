using System.Text.RegularExpressions;
using DbcParserLib.Model;
using DbcParserLib.Observers;

namespace DbcParserLib.Parsers
{
    internal class NodeLineParser : ILineParser
    {
        private const string NameGroup = "Name";
        private const string NodeLineStarter = "BU_:";

        private readonly string m_nodeLineParsingRegex = $@"BU_:(?<{NameGroup}>(?:\s+(?:[a-zA-Z_][\w]*))+)";

        private readonly IParseFailureObserver m_observer;
        private readonly Regex m_regex;

        public NodeLineParser(IParseFailureObserver observer)
        {
            m_observer = observer;
            m_regex = new Regex(m_nodeLineParsingRegex);
        }

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            if (line.TrimStart().StartsWith(NodeLineStarter) == false)
                return false;

            // Empty node list
            if (line.Trim().Equals(NodeLineStarter))
                return true;

            var match = m_regex.Match(line);
            if (match.Success)
            {
                foreach (var nodeName in match.Groups[NameGroup].Value.TrimStart().SplitBySpace())
                {
                    var node = new Node()
                    {
                        Name = nodeName
                    };
                    builder.AddNode(node);
                }
            }
            else
                m_observer.NodeSyntaxError();

            return true;
        }
    }
}