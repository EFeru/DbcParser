using System.Linq;
using System.Text.RegularExpressions;
using DbcParserLib.Model;

namespace DbcParserLib.Parsers
{
    internal class NodeLineParser : ILineParser
    {
        private const string NodeLineStarter = "BU_:";
        private const string NodeLineParsingRegex = @"BU_:((?:\s+(?:[a-zA-Z_][\w]*))*)";

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            if (line.TrimStart().StartsWith(NodeLineStarter) == false)
                return false;

            var match = Regex.Match(line, NodeLineParsingRegex);
            if (match.Success)
            {
                foreach (var nodeName in match.Groups[1].Value.TrimStart().SplitBySpace())
                {
                    var node = new Node()
                    {
                        Name = nodeName
                    };
                    builder.AddNode(node);
                }
            }
            return true;
        }
    }
}