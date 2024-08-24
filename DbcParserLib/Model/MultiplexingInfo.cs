using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DbcParserLib.Model;

public class MultiplexingInfo
{
    private const string MultiplexorLabel = "M";
    private const string MultiplexedLabel = "m";
    
    public MultiplexingRole Role { get; }
    
    /// <summary>
    /// This always exists but only contains information if Role is Multiplexed or MultiplexedMultiplexor
    /// </summary>
    public Multiplexing Multiplexing { get; }

    public MultiplexingInfo()
    {
        Role = MultiplexingRole.Unknown;
        Multiplexing = new Multiplexing();
    }
    
    public MultiplexingInfo(Signal signal, Message message, bool messageHasComplexMultiplexing)
    {
        if (!messageHasComplexMultiplexing)
        {
            HandleSimpleMultiplexing(signal, message, out var role, out var multiplexing);
            Role = role;
            Multiplexing = multiplexing;
        }
        else
        {
            HandleExtendedMultiplexing(signal, out var role, out var multiplexing);
            Role = role;
            Multiplexing = multiplexing;
        }
    }

    private void HandleSimpleMultiplexing(Signal signal, Message message, out MultiplexingRole role, out Multiplexing multiplexing)
    {
        role = MultiplexingRole.Unknown;
        multiplexing = new Multiplexing();
        
        if (string.IsNullOrWhiteSpace(signal.m_multiplexing))
        {
            role = MultiplexingRole.None;
            return;
        }
        
        if (signal.m_multiplexing.Equals(MultiplexorLabel))
        {
            role = MultiplexingRole.Multiplexor;
            return;
        }

        if (signal.m_multiplexing.EndsWith(MultiplexorLabel))
        {
            return;
        }
        
        const string extractNumberFromStringRegex = @"\d+";
        var match = Regex.Match(signal.m_multiplexing, extractNumberFromStringRegex);

        if (match.Success && uint.TryParse(match.Value, out var multiplexorValue))
        {
            var multiplexorSignal =  message.m_signals.Values.FirstOrDefault(x => x.m_multiplexing.Equals(MultiplexorLabel));
            if (multiplexorSignal is null)
            {
                return;
            }
            role = MultiplexingRole.Multiplexed;
            multiplexing = new Multiplexing
            {
                MultiplexorSignal = multiplexorSignal.Name,
                MultiplexorRanges = new List<MultiplexorRange> { new MultiplexorRange
                    {
                        Lower = multiplexorValue,
                        Upper = multiplexorValue
                    }
                }
            };
        }
    }
    
    private void HandleExtendedMultiplexing(Signal signal, out MultiplexingRole role, out Multiplexing multiplexing)
    {
        role = MultiplexingRole.Unknown;
        multiplexing = new Multiplexing();
        
        if (string.IsNullOrWhiteSpace(signal.m_multiplexing))
        {
            role = MultiplexingRole.None;
            return;
        }

        if (signal.m_multiplexing.Equals(MultiplexorLabel))
        {
            role = MultiplexingRole.Multiplexor;
            return;
        }

        if (signal.m_multiplexing.StartsWith(MultiplexedLabel))
        {
            if (signal.m_multiplexing.EndsWith(MultiplexorLabel))
            {
                if (signal.m_extendedMultiplex is not null)
                {
                    role = MultiplexingRole.MultiplexedMultiplexor;
                    multiplexing = signal.m_extendedMultiplex;
                    return;
                }
                
                role = MultiplexingRole.Unknown;
                return;
            }

            if (signal.m_extendedMultiplex is not null)
            {
                role = MultiplexingRole.Multiplexed;
                multiplexing = signal.m_extendedMultiplex;
            }
        }
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

public class Multiplexing
{
    public string MultiplexorSignal { get; internal set; } = string.Empty;
    public IReadOnlyCollection<MultiplexorRange> MultiplexorRanges { get; internal set; } = new List<MultiplexorRange>();
}

public class MultiplexorRange
{
    public uint Lower { get; internal set; }
    public uint Upper { get; internal set; }
}