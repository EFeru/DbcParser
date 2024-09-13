namespace DbcParserLib.Parsers.Evo
{
    public interface ITextFragment
    {
        bool TryPeek(out char item);
        void Pop();
        bool RequiresNewLine { get; }
        bool IsNew { get; }
        int Col { get; }
    }
}