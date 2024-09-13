using DbcParserLib.Parsers.Evo.Parsers;

namespace DbcParserLib.Parsers.Evo
{
    public interface IKeywordStore
    {
        bool TryGetKeywordParser(string keyword, out IKeywordParser parser);
    }
}