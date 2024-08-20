namespace DbcParserLib.Model;

public class MultiplexingInfo
{
    private const string MultiplexorLabel = "M";
    private const string MultiplexedLabel = "m";

    public MultiplexingRole Role { get; }
    public int Group { get; }

    public MultiplexingInfo(Signal signal)
    {
        Role = ParseMultiplexingInfo(signal, out var group);
        Group = group;
    }

    private static MultiplexingRole ParseMultiplexingInfo(Signal signal, out int multiplexingGroup)
    {
        multiplexingGroup = 0;
        if (string.IsNullOrWhiteSpace(signal.multiplexing))
        {
            return MultiplexingRole.None;
        }

        if (signal.multiplexing.Equals(MultiplexorLabel))
        {
            return MultiplexingRole.Multiplexor;
        }

        if (signal.multiplexing.StartsWith(MultiplexedLabel))
        {
            var substringLength = signal.multiplexing.Length - (signal.multiplexing.EndsWith(MultiplexorLabel) ? 2 : 1);

            if (int.TryParse(signal.multiplexing.Substring(1, substringLength), out multiplexingGroup))
            {
                return MultiplexingRole.Multiplexed;
            }
        }

        return MultiplexingRole.Unknown;
    }
}

public enum MultiplexingRole
{
    None,
    Unknown,
    Multiplexed,
    Multiplexor
}