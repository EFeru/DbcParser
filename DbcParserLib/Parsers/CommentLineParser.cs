using System;
using System.ComponentModel;
using System.Linq;
using static DbcParserLib.Parser;

namespace DbcParserLib.Parsers
{
    public class CommentLineParser : ILineParser
    {
        private const string CommentLineStarter = "CM_ ";
        private static bool isMultiline = false;

        public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            var cleanLine = line.Trim(' ');

            if (cleanLine.StartsWith(CommentLineStarter) == false)
                return false;

            if (!cleanLine.EndsWith(";"))
                isMultiline = true;
            else
                cleanLine = cleanLine.Trim(';');

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

            isMultiline= false;
            return false;
        }

        private static void SetSignalComment(string sigCommentStr, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            string[] records = sigCommentStr.SplitBySpace();
            if (records.Length > 4 && uint.TryParse(records[2], out var messageId))
            {
                var comment = string.Join(Helpers.Space, records.Skip(4)).Trim(' ', '"', ';');
                if (isMultiline)
                    comment = GetNextLine(comment, nextLineProvider);
                builder.AddSignalComment(messageId, records[3], comment);
            }
        }

        private static void SetNodeComment(string sigCommentStr, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            string[] records = sigCommentStr.SplitBySpace();
            if (records.Length > 3)
            {
                var comment = string.Join(Helpers.Space, records.Skip(3)).Trim(' ', '"', ';');
                if (isMultiline)
                    comment = GetNextLine(comment, nextLineProvider);
                builder.AddNodeComment(records[2].Trim(), comment);
            }
        }

        private static void SetMessageComment(string sigCommentStr, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            string[] records = sigCommentStr.SplitBySpace();
            if (records.Length > 3 && uint.TryParse(records[2], out var messageId))
            {
                var comment = string.Join(Helpers.Space, records.Skip(3)).Trim(' ', '"', ';');
                if (isMultiline)
                    comment = GetNextLine(comment, nextLineProvider);
                builder.AddMessageComment(messageId, comment);
            }
        }

        private static string GetNextLine(string currentLine, INextLineProvider nextLineProvider)
        {
            string nextLine;
            while (nextLineProvider.TryGetLine(out nextLine))
            {
                currentLine = string.Join(Helpers.Space, currentLine, nextLine.Trim(' ', '"'));
                if (nextLine.EndsWith(";"))
                    break;
            }
            return currentLine.Trim('"', ';');
        }

    }
}