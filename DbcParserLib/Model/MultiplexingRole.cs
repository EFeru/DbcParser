namespace DbcParserLib.Model
{
    public enum MultiplexingRole
    {
        None, 
        Unknown, // Used if parsing fails
        Multiplexed, 
        Multiplexor, 
        MultiplexedMultiplexor
    }
}
