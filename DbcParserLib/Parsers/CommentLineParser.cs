using System.Text;
using System.Text.RegularExpressions;

namespace DbcParserLib.Parsers
{
    internal class CommentLineParser : ILineParser
    {
        private const string CommentLineStarter = "CM_ ";
        private const string NodeParsingRegex = @"CM_ BU_\s+([a-zA-Z_][\w]*)\s+""*([^""]*)""*\s*;";
        private const string MessageParsingRegex = @"CM_ BO_\s+(\d+)\s+""*([^""]*)""*\s*;";
        private const string SignalParsingRegex = @"CM_ SG_\s+(\d+)\s+([a-zA-Z_][\w]*)\s+""*([^""]*)""*\s*;";
        private const string EnvironmentVariableParsingRegex = @"CM_ EV_\s+([a-zA-Z_][\w]*)\s+""*([^""]*)""*\s*;";

        private readonly IParseObserver m_observer;

        public CommentLineParser(IParseObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, int lineNumber, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var cleanLine = line.Trim();

            if (cleanLine.StartsWith(CommentLineStarter) == false)
                return false;

            if (!cleanLine.EndsWith(";"))
                cleanLine = GetNextLines(cleanLine, nextLineProvider);

            if (cleanLine.StartsWith("CM_ SG_"))
            {
                SetSignalComment(cleanLine, builder, nextLineProvider);
                return true;
            }

            if (cleanLine.StartsWith("CM_ BU_"))
            {
                SetNodeComment(cleanLine, builder, nextLineProvider);
                return true;
            }

            if (cleanLine.StartsWith("CM_ BO_"))
            {
                SetMessageComment(cleanLine, builder, nextLineProvider);
                return true;
            }

            if (cleanLine.StartsWith("CM_ EV_"))
            {
                SetEnvironmentVariableComment(cleanLine, builder);
                return true;
            }

            m_observer.CommentSintaxError(lineNumber);
            return false;
        }

        private static void SetSignalComment(string sigCommentStr, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var match = Regex.Match(sigCommentStr, SignalParsingRegex);

            if (match.Success)
            {
                builder.AddSignalComment(uint.Parse(match.Groups[1].Value), match.Groups[2].Value, match.Groups[3].Value);
            }
        }

        private static void SetNodeComment(string sigCommentStr, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var match = Regex.Match(sigCommentStr, NodeParsingRegex);

            if (match.Success)
            {
                builder.AddNodeComment(match.Groups[1].Value, match.Groups[2].Value);
            }
        }

        private static void SetMessageComment(string sigCommentStr, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var match = Regex.Match(sigCommentStr, MessageParsingRegex);

            if (match.Success)
            {
                builder.AddMessageComment(uint.Parse(match.Groups[1].Value), match.Groups[2].Value);
            }
        }

        private static void SetEnvironmentVariableComment(string envCommentStr, IDbcBuilder builder)
        {
            var match = Regex.Match(envCommentStr, EnvironmentVariableParsingRegex);

            if (match.Success)
            {
                builder.AddEnvironmentVariableComment(match.Groups[1].Value, match.Groups[2].Value);
            }
        }

        private static string GetNextLines(string currentLine, INextLineProvider nextLineProvider)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(currentLine);

            while (nextLineProvider.TryGetLine(out var nextLine))
            {
                stringBuilder.AppendLine(nextLine);
                if (nextLine.EndsWith(";"))
                    break;
            }
            return stringBuilder.ToString();
        }

    }
}