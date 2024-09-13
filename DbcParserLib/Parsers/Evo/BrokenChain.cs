using System;

namespace DbcParserLib.Parsers.Evo
{
    public class BrokenChain : IChainBuilder
    {
        public static readonly IChainBuilder Instance = new BrokenChain();

        private BrokenChain()
        {
            
        }

        public IChainBuilder Vacuum(bool optional = false)
        {
            return Instance;
        }

        public IChainBuilder One(char value)
        {
            return Instance;
        }

        public IChainBuilder Read(Predicate<char> take, out string text)
        {
            text = null;
            return Instance;
        }

        public IChainBuilder ReadId(out string text)
        {
            text = null;
            return Instance;
        }

        public IChainBuilder OneOf(Predicate<char> filter)
        {
            return Instance;
        }

        public bool Assert()
        {
            return false;
        }
    }
}