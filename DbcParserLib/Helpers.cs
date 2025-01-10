using System.Text;
using System.Text.RegularExpressions;

namespace DbcParserLib
{
    public static class Helpers
    {
        public const string Space = " ";
        public const string Comma = ",";
        public const string DoubleQuotes = "\"";

        private static readonly string[] SpaceArray = { Space };

        public static string[] SplitBySpace(this string value)
        {
            return value.Split(SpaceArray, System.StringSplitOptions.RemoveEmptyEntries);
        }
        
        public static string ConcatenateTextComment(string[] strings, int startingIndex)
        {
            var sb = new StringBuilder();
            foreach(var s in strings)
            {
                sb.AppendLine(Regex.Replace(s, @"""", ""));
            }
            var commentText = sb.ToString().Substring(startingIndex);
            return commentText.Substring(0, commentText.Length - 3);
        }
    }
}