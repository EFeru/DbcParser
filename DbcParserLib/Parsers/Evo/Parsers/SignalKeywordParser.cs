using System.Collections.Generic;

namespace DbcParserLib.Parsers.Evo.Parsers
{
    public class SignalKeywordParser : IKeywordParser
    {
        //  SG_ EngineSpeed : 24|16@1+(0.125,0)[0|8031.875] "rpm" Vector_XXX
        public IKeywordParser TryParse(TextBrowser browser, IKeywordStore store)
        {
            var result = browser.CompactChain()
                .ReadId(out var signalId)
                .One(CharExtensions.Colon)
                .Read(char.IsDigit, out var signalOffset)
                .One('|')
                .Read(char.IsDigit, out var signalLength)
                .One('@')
                .Read(char.IsDigit, out var endianness)
                .Read(c => c == '+' || c == '-', out var signed)
                .One('(')
                .Read(c => c.IsDot() || char.IsDigit(c), out var scale)
                .One(',')
                .Read(c => c.IsDot() || char.IsDigit(c), out var offset)
                .One(')')
                .One('[')
                .Read(c => c.IsDot() || char.IsDigit(c), out var min)
                .One('|')
                .Read(c => c.IsDot() || char.IsDigit(c), out var max)
                .One(']')
                .One(CharExtensions.DoubleQuote)
                .Read(c => char.IsWhiteSpace(c) == false && c != CharExtensions.DoubleQuote, out var unit)
                .One(CharExtensions.DoubleQuote)
                .ReadId(out var rx)
                .Assert();

            if (result)
            {
                var rxNodes = new List<string>() { rx };
                while (browser.Chain().Vacuum(true).One(CharExtensions.Comma).Vacuum(true).Assert())
                {
                    if (browser.TryReadId(out var rxId))
                    {
                        rxNodes.Add(rxId);
                    }

                    // Create Signal
                }

                return new GenericNextKeywordFinder();
            }
            return null;
        }
    }
}