using System.Text;

namespace DbcParserLib
{
    public static class Helpers
    {
        private static readonly string[] SpaceArray = new[] { " " };
        public const string Space = " ";
        public const string Comma = ",";

        public static string[] SplitBySpace(this string value)
        {
            return value.Split(SpaceArray, System.StringSplitOptions.RemoveEmptyEntries);
        }

        public static string ConvertToMultiLine(string[] records, int offset)
        {
            var sb = new StringBuilder();
            for (var i = offset; i < records.Length - 1; i += 2)
            {
                sb.AppendLine($"{records[i]} {records[i + 1]}");
            }

            return sb.ToString();
        }
    }

}