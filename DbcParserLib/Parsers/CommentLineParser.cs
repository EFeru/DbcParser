using System;

namespace DbcParserLib.Parsers
{
    public class CommentLineParser : ILineParser
    {
        private const string CommentLineStarter = "CM_";

        public bool TryParse(string line, DbcBuilder builder)
        {
            if(line.TrimStart().StartsWith(CommentLineStarter) == false)
                return false;

            if (line.TrimStart().StartsWith("CM_ SG_ "))
            {
                SetSignalComment(line, builder);
                return true;
            }
            
            if (line.TrimStart().StartsWith("CM_ BU_ "))
            {
                SetNodeComment(line, builder);
                return true;
            }

            if (line.TrimStart().StartsWith("CM_ BO_ "))
            {
                SetMessageComment(line, builder);
                return true;
            }

            return false;
        }

        private void SetSignalComment(string sigCommentStr, DbcBuilder builder)
        {
            string[] records = sigCommentStr.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if(records.Length >= 5 && uint.TryParse(records[2], out var messageId))
            {
                builder.AddSignalComment(messageId, records[3], records[4].Trim(' ', '"', ';'));
            }
        }

        private void SetNodeComment(string sigCommentStr, DbcBuilder builder)
        {
            string[] records = sigCommentStr.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if(records.Length == 3)
            {
                builder.AddNodeComment(records[1].Trim(), records[2].Trim(' ', '"', ';'));
            }
        }

        private void SetMessageComment(string sigCommentStr, DbcBuilder builder)
        {
            string[] records = sigCommentStr.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if(records.Length == 4 && uint.TryParse(records[2], out var messageId))
            {
                builder.AddMessageComment(messageId, records[3].Trim(' ', '"', ';'));
            }
        }
    }
}