using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DbcParserLib.Model;
using DbcParserLib.Observers;

namespace DbcParserLib.Parsers;

internal class ExtendedMultiplexorLineParser : ILineParser
{
    private const string SignalLineStarter = "SG_MUL_VAL_ ";
    private const string SignalRegex = @"SG_MUL_VAL_\s+(\d+)\s+(\S+)\s+(\S+)\s+((?:\d+-\d+,?\s*)+);?";

    private readonly IParseFailureObserver m_observer;
    
    public ExtendedMultiplexorLineParser(IParseFailureObserver observer)
    {
        m_observer = observer;
    }

    public bool TryParse(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
    {
        if (!line.TrimStart().StartsWith(SignalLineStarter))
        {
            return false;
        }
        
        var match = Regex.Match(line, SignalRegex);

        if (match.Success)
        {
            var multiplexorRanges = match.Groups[4].Value;
            var rangesArray = multiplexorRanges.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            var rangesParsed = new List<MultiplexorRange>();
            
            foreach (var range in rangesArray)
            {
                var rangeClean = range.Trim();
                var numbers = rangeClean.Split('-');

                var multiplexorRange = new MultiplexorRange
                {
                    Lower = uint.Parse(numbers[0]),
                    Upper = uint.Parse(numbers[1])
                };

                rangesParsed.Add(multiplexorRange);
            }
            
            builder.AddSignalExtendedMultiplexingInfo(uint.Parse(match.Groups[1].Value), match.Groups[2].Value, match.Groups[3].Value, rangesParsed);
        }
        else
        {
            m_observer.SignalMultiplexingError();
        }

        return true;
    }
}