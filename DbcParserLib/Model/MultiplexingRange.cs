namespace DbcParserLib.Model
{
    public class MultiplexingRange
    {
        public bool IsRange { get; }
        public uint Lower { get; }
        public uint Upper { get; }

        public MultiplexingRange(uint lower, uint upper)
        {
            Lower = lower;
            Upper = upper;
            IsRange = lower != upper;
        }
    }
}
