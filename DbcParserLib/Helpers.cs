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
    }

}