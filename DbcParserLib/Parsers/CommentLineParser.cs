using System.Text;
using System.Text.RegularExpressions;
using DbcParserLib.Observers;

namespace DbcParserLib.Parsers
{
    internal class CommentLineParser : ILineParser
    {
        private const string CharGroup = "CharString";
        private const string NodeNameGroup = "NodeName";
        private const string MessageIdGroup = "MessageId";
        private const string SignalNameGroup = "SignalName";
        private const string EnvVarNameGroup = "EnvVarName";
        private const string CommentLineStarter = "CM_ ";

        private readonly string m_genericCommentParsingRegex = $@"CM_\s+""*(?<{CharGroup}>[^""]*)""*\s*;";
        private readonly string m_nodeParsingRegex = $@"CM_ BU_\s+(?<{NodeNameGroup}>[a-zA-Z_][\w]*)\s+""*(?<{CharGroup}>[^""]*)""*\s*;";
        private readonly string m_messageParsingRegex = $@"CM_ BO_\s+(?<{MessageIdGroup}>\d+)\s+""*(?<{CharGroup}>[^""]*)""*\s*;";
        private readonly string m_signalParsingRegex = $@"CM_ SG_\s+(?<{MessageIdGroup}>\d+)\s+(?<{SignalNameGroup}>[a-zA-Z_][\w]*)\s+""*(?<{CharGroup}>[^""]*)""*\s*;";
        private readonly string m_environmentVariableParsingRegex = $@"CM_ EV_\s+(?<{EnvVarNameGroup}>[a-zA-Z_][\w]*)\s+""*(?<{CharGroup}>[^""]*)""*\s*;";

        private readonly IParseFailureObserver m_observer;

        public CommentLineParser(IParseFailureObserver observer)
        {
            m_observer = observer;
        }

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var cleanLine = line.Trim();

            if (cleanLine.StartsWith(CommentLineStarter) == false)
                return false;

            if (!cleanLine.EndsWith(";"))
                cleanLine = GetNextLines(cleanLine, m_observer, nextLineProvider);

            if (cleanLine.StartsWith("CM_ SG_"))
            {
                SetSignalComment(cleanLine, m_observer, builder, nextLineProvider);
                return true;
            }

            if (cleanLine.StartsWith("CM_ BU_"))
            {
                SetNodeComment(cleanLine, m_observer, builder, nextLineProvider);
                return true;
            }

            if (cleanLine.StartsWith("CM_ BO_"))
            {
                SetMessageComment(cleanLine, m_observer, builder, nextLineProvider);
                return true;
            }

            if (cleanLine.StartsWith("CM_ EV_"))
            {
                SetEnvironmentVariableComment(cleanLine, m_observer, builder);
                return true;
            }

            var match = Regex.Match(cleanLine, m_genericCommentParsingRegex);
            if (match.Success)
                return true;

            m_observer.CommentSyntaxError();
            return true;
        }

        private void SetSignalComment(string sigCommentStr, IParseFailureObserver observer, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var match = Regex.Match(sigCommentStr, m_signalParsingRegex);

            if (match.Success)
                builder.AddSignalComment(uint.Parse(match.Groups[MessageIdGroup].Value), match.Groups[SignalNameGroup].Value, match.Groups[CharGroup].Value);
            else
                observer.CommentSyntaxError();
        }

        private void SetNodeComment(string sigCommentStr, IParseFailureObserver observer, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var match = Regex.Match(sigCommentStr, m_nodeParsingRegex);

            if (match.Success)
                builder.AddNodeComment(match.Groups[NodeNameGroup].Value, match.Groups[CharGroup].Value);
            else
                observer.CommentSyntaxError();
        }

        private void SetMessageComment(string sigCommentStr, IParseFailureObserver observer, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var match = Regex.Match(sigCommentStr, m_messageParsingRegex);

            if (match.Success)
                builder.AddMessageComment(uint.Parse(match.Groups[MessageIdGroup].Value), match.Groups[CharGroup].Value);
            else
                observer.CommentSyntaxError();
        }

        private void SetEnvironmentVariableComment(string envCommentStr, IParseFailureObserver observer, IDbcBuilder builder)
        {
            var match = Regex.Match(envCommentStr, m_environmentVariableParsingRegex);

            if (match.Success)
                builder.AddEnvironmentVariableComment(match.Groups[EnvVarNameGroup].Value, match.Groups[CharGroup].Value);
            else
                observer.CommentSyntaxError();
        }

        private static string GetNextLines(string currentLine, IParseFailureObserver observer, INextLineProvider nextLineProvider)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(currentLine);

            while (nextLineProvider.TryGetLine(out var nextLine))
            {
                observer.CurrentLine++;
                stringBuilder.AppendLine(nextLine);
                if (nextLine.EndsWith(";"))
                    break;
            }
            return stringBuilder.ToString();
        }

    }
}