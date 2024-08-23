using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DbcParserLib.Model;

public class MultiplexingInfo
{
    private const string MultiplexorLabel = "M";
    private const string MultiplexedLabel = "m";

    public MultiplexingMode Mode { get; private set; }
    
    /// <summary>
    /// As of now a Multiplexor will always lead to Mode being Simple
    /// </summary>
    public MultiplexingRole Role { get; private set; }
    
    /// <summary>
    /// Is available if Mode is Simple and the Role is Multiplexed 
    /// </summary>
    public SimpleMultiplex? SimpleMultiplex { get; private set; }
    
    /// <summary>
    /// Is available if Mode is Extended and Role is Multiplexed or MultiplexedMultiplexor
    /// </summary>
    public ExtendedMultiplex? ExtendedMultiplex { get; private set; }

    public MultiplexingInfo()
    {
        Mode = MultiplexingMode.Unknown;
        Role = MultiplexingRole.Unknown;
    }
    
    public MultiplexingInfo(Signal signal)
    {
        Mode = GetMultiplexingMode(signal);
        ParseMultiplexing(signal);
    }

    private static MultiplexingMode GetMultiplexingMode(Signal signal)
    {
        if (string.IsNullOrWhiteSpace(signal.multiplexing))
        {
            return MultiplexingMode.None;
        }

        if (signal.extendedMultiplex is not null)
        {
            return MultiplexingMode.Extended;
        }
        
        return MultiplexingMode.Simple;
    }
    
    private void ParseMultiplexing(Signal signal)
    {
        if (string.IsNullOrWhiteSpace(signal.multiplexing))
        {
            Role = MultiplexingRole.None;
            return;
        }

        if (signal.multiplexing.Equals(MultiplexorLabel))
        {
            Role = MultiplexingRole.Multiplexor;
            return;
        }

        if (signal.multiplexing.StartsWith(MultiplexedLabel))
        {
            if (signal.multiplexing.EndsWith(MultiplexorLabel))
            {
                if (signal.extendedMultiplex is not null)
                {
                    Role = MultiplexingRole.MultiplexedMultiplexor;
                    ExtendedMultiplex = signal.extendedMultiplex;
                    return;
                }
                
                Role = MultiplexingRole.Unknown;
                return;
            }

            if (signal.extendedMultiplex is not null)
            {
                Role = MultiplexingRole.Multiplexed;
                ExtendedMultiplex = signal.extendedMultiplex;
                return;
            }
            
            const string extractNumberFromStringRegex = @"\d+";
            var match = Regex.Match(signal.multiplexing, extractNumberFromStringRegex);

            if (match.Success && uint.TryParse(match.Value, out var multiplexorValue))
            {
                Role = MultiplexingRole.Multiplexed;
                SimpleMultiplex = new SimpleMultiplex
                {
                    MultiplexorValue = multiplexorValue
                };
                return;
            }
        }

        Role = MultiplexingRole.Unknown;
    }
}

public enum MultiplexingRole
{
    None,
    Unknown,
    Multiplexed,
    Multiplexor,
    MultiplexedMultiplexor
}

public enum MultiplexingMode
{
    None,
    Unknown,
    Simple,
    Extended
}

public class SimpleMultiplex
{
    public uint MultiplexorValue { get; internal set; }
}

public class ExtendedMultiplex
{
    public string MultiplexorSignal { get; internal set; } = string.Empty;
    public IReadOnlyCollection<MultiplexorRange> MultiplexorRanges { get; internal set; } = new List<MultiplexorRange>();
}

public class MultiplexorRange
{
    public uint Lower { get; internal set; }
    public uint Upper { get; internal set; }
}