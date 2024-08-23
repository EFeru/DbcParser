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
    public IReadOnlyDictionary<string, Signal> Signals => signals;
    internal Dictionary<string, Signal> signals = new ();
    public IReadOnlyDictionary<string, CustomProperty> CustomProperties => customProperties;
    internal readonly Dictionary<string, CustomProperty> customProperties = new ();

    internal void FinishUp()
    {
        AdjustExtendedId();
        var hasCycleTime = TryGetCycleTime(out var cycleTime);
        CycleTime = hasCycleTime ? cycleTime : null;
        
        foreach (var signal in signals.Values)
        {
            signal.FinishUp();
            signal.MessageID = ID;
        }
        IsMultiplexed = signals.Values.Any(s => s.Multiplexing.Role == MultiplexingRole.Multiplexor);
    }
    
    private bool TryGetCycleTime(out int cycleTime)
    {
        cycleTime = 0;

        if (customProperties.TryGetValue("GenMsgCycleTime", out var property))
        {
            if (property.PropertyValue is not IntegerPropertyValue integerPropertyValue)
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