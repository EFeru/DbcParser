using System.IO;
using System.Collections.Generic;
using DbcParserLib.Parsers;
using System.Reflection;
using System.Xml;

namespace DbcParserLib
{
    public interface INextLineProvider
    {
        bool TryGetLine(out string line);
    }

    public class NextLineProvider : INextLineProvider
    {
        private TextReader m_reader;

        public NextLineProvider(TextReader reader)
        {
            m_reader = reader;
        }

        public bool TryGetLine(out string line)
        {
            line = null;
            if (m_reader.Peek() >= 0)
            {
                line = m_reader.ReadLine();
                return true;
            }
            return false;
        }
    }

    public static class Parser
    {
        private static IEnumerable<ILineParser> LineParsers = new List<ILineParser>()
        {
            new IgnoreLineParser(), // Used to skip line we know we want to skip
            new NodeLineParser(),
            new MessageLineParser(),
            new CommentLineParser(),
            new SignalLineParser(),
            new SignalValueTypeLineParser(),
            new ValueTableLineParser(),
            new PropertiesLineParser(),
            new UnknownLineParser() // Used as a catch all 
        };
        
        public static Dbc ParseFromPath(string dbcPath)
        {
            using (var fileStream = new FileStream(dbcPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return ParseFromStream(fileStream);
            }
        }

        public static Dbc ParseFromStream(Stream dbcStream)
        {
            using(var reader = new StreamReader(dbcStream))
            {
                return ParseFromReader(reader);
            }
        }

        public static Dbc Parse(string dbcText)
        {
            using(var reader = new StringReader(dbcText))
            {
                return ParseFromReader(reader);
            }
        }

        private static Dbc ParseFromReader(TextReader reader)
        {
            var builder = new DbcBuilder();
            var nextLineProvider = new NextLineProvider(reader);

            while (reader.Peek() >= 0)
                ParseLine(reader.ReadLine(), builder, nextLineProvider);

            return builder.Build();
        }

        private static void ParseLine(string line, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            if(string.IsNullOrWhiteSpace(line))
                return;

            
            foreach(var parser in LineParsers)
            {
                if(parser.TryParse(line, builder, nextLineProvider))
                    break;
            }
        }
    }
}