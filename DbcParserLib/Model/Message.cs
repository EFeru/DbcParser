using System.Collections.Generic;
using System.Linq;

namespace DbcParserLib.Model;

public class Message
{
    public uint ID { get; internal set; }
    public bool IsExtID { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public ushort DLC { get; internal set; }
    public string Transmitter { get; internal set; } = string.Empty;
    public string Comment { get; internal set; } = string.Empty;
    public bool IsMultiplexed { get; private set; }
    public int? CycleTime { get; private set; }
    public IReadOnlyDictionary<string, Signal> Signals => m_signals;
    internal Dictionary<string, Signal> m_signals = new ();
    public IReadOnlyDictionary<string, CustomProperty> CustomProperties => m_customProperties;
    internal readonly Dictionary<string, CustomProperty> m_customProperties = new ();

    internal void FinishUp()
    {
        AdjustExtendedId();
        var hasCycleTime = TryGetCycleTime(out var cycleTime);
        CycleTime = hasCycleTime ? cycleTime : null;

        var hasExtendedMultiplexing = m_signals.Values.Any(x => x.m_extendedMultiplex is not null);
        foreach (var signal in m_signals.Values)
        {
            signal.FinishUp(this, hasExtendedMultiplexing);
            signal.MessageID = ID;
        }
        IsMultiplexed = m_signals.Values.Any(s => s.MultiplexingInfo.Role == MultiplexingRole.Multiplexor);
    }
    
    private bool TryGetCycleTime(out int cycleTime)
    {
        cycleTime = 0;

        if (m_customProperties.TryGetValue("GenMsgCycleTime", out var propertyCycleTime))
        {
            var foundSendType = m_customProperties.TryGetValue("GenMsgSendType", out var propertySendType);
            if (foundSendType)
            {
                if (propertySendType!.PropertyValue is EnumPropertyValue enumPropertyValue)
                {
                    if (!enumPropertyValue.Value.ToLower().Contains("cyclic"))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            
            if (propertyCycleTime.PropertyValue is not IntegerPropertyValue integerPropertyValue)
            {
                return false;
            }
            cycleTime = integerPropertyValue.Value;
            return true;
        }

        return false;
    }

    private void AdjustExtendedId()
    {
        // For extended ID bit 31 is always 1
        if (ID >= 0x80000000)
        {
            IsExtID = true;
            ID -= 0x80000000;
        }
    }
}