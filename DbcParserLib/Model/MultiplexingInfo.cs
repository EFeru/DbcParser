namespace DbcParserLib.Model
{
    public class MultiplexingInfo
    { 
        public MultiplexingRole Role { get; }
        public int Group { get; }

        public MultiplexingInfo(MultiplexingRole role)
            : this(role, 0)
        {
        }

        public MultiplexingInfo(MultiplexingRole role, int group)
        {
            Role = role;
            Group = group;
        }
    }
}
