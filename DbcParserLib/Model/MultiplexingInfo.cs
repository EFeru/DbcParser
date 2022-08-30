namespace DbcParserLib.Model
{
    public struct MultiplexingInfo
    {
        public MultiplexingInfo(MultiplexingRole role)
            : this(role, 0)
        {
        }

        public MultiplexingInfo(MultiplexingRole role, int group)
        {
            Role = role;
            Group = group;
        }

        public MultiplexingRole Role {get;}
        public int Group {get;}
    }

    public enum MultiplexingRole
    {
        None, Unknown, Multiplexed, Multiplexor
    }
}
