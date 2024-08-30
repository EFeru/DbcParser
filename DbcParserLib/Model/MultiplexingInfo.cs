using System.Linq;

namespace DbcParserLib.Model
{
    public class MultiplexingInfo
    { 
        public MultiplexingRole Role { get; }
        public uint Group { get; }

        public MultiplexingInfo(ParsingMultiplexing parsingMultiplexing)
        {
            Role = parsingMultiplexing.Role;
            Group = parsingMultiplexing.MultiplexerValues.FirstOrDefault();
        }
    }
}
