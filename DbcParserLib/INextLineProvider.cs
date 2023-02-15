namespace DbcParserLib
{
    public interface INextLineProvider
    {
        bool TryGetLine(out string line);
    }
}
