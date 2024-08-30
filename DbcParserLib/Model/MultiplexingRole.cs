using System;

namespace DbcParserLib.Model
{
    [Flags]
    public enum MultiplexingRole
    {
        None, 
        Multiplexed, 
        Multiplexor
    }
}
