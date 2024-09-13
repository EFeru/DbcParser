using System;

namespace DbcParserLib.Parsers.Evo
{
    public interface IChainBuilder
    {
        IChainBuilder Vacuum(bool optional = false);
        IChainBuilder One(char value);
        IChainBuilder Read(Predicate<char> take, out string text);
        IChainBuilder ReadId(out string text);
        IChainBuilder OneOf(Predicate<char> filter);
        //IChainBuilder Many(char value);
        //IChainBuilder ManyOf(Predicate<char> filter);

        bool Assert();
    }
}