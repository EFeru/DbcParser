using System;
using System.Linq;

namespace DbcParserLib.Parsers
{
    public class CommentLineParser : ILineParser
    {
        private const string CommentLineStarter = "CM_ ";

        public bool TryParse(string line, IDbcBuilder builder)
        {
            var cleanLine = line.Trim(' ', ';');

            if (cleanLine.StartsWith(CommentLineStarter) == false)
                return false;

            if (cleanLine.StartsWith("CM_ SG_"))
            {
                SetSignalComment(cleanLine, builder);
                return true;
            }
            
            if (cleanLine.StartsWith("CM_ BU_"))
            {
                SetNodeComment(cleanLine, builder);
                return true;
            }

            if (cleanLine.StartsWith("CM_ BO_"))
            {
                SetMessageComment(cleanLine, builder);
                return true;
            }

            return false;
        }

        private static void SetSignalComment(string sigCommentStr, IDbcBuilder builder)
        {
            string[] records = sigCommentStr.SplitBySpace();
            if(records.Length > 4 && uint.TryParse(records[2], out var messageId))
            {
                builder.AddSignalComment(messageId, records[3], string.Join(Helpers.Space, records.Skip(4)).Trim(' ', '"', ';'));
            }
        }

        private static void SetNodeComment(string sigCommentStr, IDbcBuilder builder)
        {
            string[] records = sigCommentStr.SplitBySpace();
            if (records.Length > 3)
            {
                builder.AddNodeComment(records[2].Trim(), string.Join(Helpers.Space, records.Skip(3)).Trim(' ', '"', ';'));
            }
        }

        private static void SetMessageComment(string sigCommentStr, IDbcBuilder builder)
        {
            string[] records = sigCommentStr.SplitBySpace();
            if (records.Length > 3 && uint.TryParse(records[2], out var messageId))
            {
                builder.AddMessageComment(messageId, string.Join(Helpers.Space, records.Skip(3)).Trim(' ', '"', ';'));
            }
        }
    }
}