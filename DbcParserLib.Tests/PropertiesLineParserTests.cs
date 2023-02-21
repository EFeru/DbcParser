using NUnit.Framework;
using DbcParserLib.Parsers;
using DbcParserLib.Model;
using Moq;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace DbcParserLib.Tests
{
    public class PropertiesLineParserTests
    {
        private static List<ILineParser> CreateParser()
        {
            return new List<ILineParser>() {
                new PropertiesLineParser(),
                new PropertiesDefinitionLineParser()
            };
        }

        private static bool ParseLine(string line, List<ILineParser> lineParser, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            foreach (var parser in lineParser)
            {
                if (parser.TryParse(line, builder, nextLineProvider))
                    return true;
            }
            return false;
        }

        [Test]
        public void MsgCycleTimeIsParsed()
        {
            var builder = new DbcBuilder();
            var message = new Message { ID = 2394947585 };
            message.IsExtID = DbcBuilder.IsExtID(ref message.ID);
            builder.AddMessage(message);

            var msgCycleTimeLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_ ""GenMsgCycleTime"" BO_ 2394947585 100;", msgCycleTimeLineParser, builder, nextLineProvider));

            var dbc = builder.Build();
            Assert.AreEqual(100, dbc.Messages.First().CycleTime);
        }

        [Test]
        public void SigInitalValueIsParsed()
        {
            var builder = new DbcBuilder();
            var message = new Message { ID = 2394947585 };
            message.IsExtID = DbcBuilder.IsExtID(ref message.ID);
            builder.AddMessage(message);
            var signal = new Signal { Name = "sig_name" };
            builder.AddSignal(signal);

            var sigInitialValueLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_ ""GenSigStartValue"" SG_ 2394947585 sig_name 40;", sigInitialValueLineParser, builder, nextLineProvider));

            var dbc = builder.Build();
            Assert.AreEqual(40, dbc.Messages.First().Signals.First().InitialValue);
        }

        [Test]
        public void MsgCustomPropertyIsParsed()
        {
            var builder = new DbcBuilder();
            var message = new Message { ID = 2394947585 };
            message.IsExtID = DbcBuilder.IsExtID(ref message.ID);
            builder.AddMessage(message);

            var msgCycleTimeLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ BO_ ""GenMsgCycleTime"" INT 0 200;", msgCycleTimeLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""GenMsgCycleTime"" 150;", msgCycleTimeLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_ ""GenMsgCycleTime"" BO_ 2394947585 100;", msgCycleTimeLineParser, builder, nextLineProvider));

            var dbc = builder.Build();
            Assert.AreEqual(100, dbc.Messages.First().CycleTime);
        }

        [Test]
        public void SigCustomPropertyIsParsed()
        {
            var builder = new DbcBuilder();
            var message = new Message { ID = 2394947585 };
            message.IsExtID = DbcBuilder.IsExtID(ref message.ID);
            builder.AddMessage(message);
            var signal = new Signal { Name = "sig_name" };
            builder.AddSignal(signal);

            var sigInitialValueLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ SG_ ""GenSigStartValue"" INT 0 200;", sigInitialValueLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""GenSigStartValue"" 150;", sigInitialValueLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_ ""GenSigStartValue"" SG_ 2394947585 sig_name 40;", sigInitialValueLineParser, builder, nextLineProvider));

            var dbc = builder.Build();
            Assert.AreEqual(40, dbc.Messages.First().Signals.First().InitialValue);
        }

        [Test]
        public void NodeCustomPropertyIsParsed()
        {
            var builder = new DbcBuilder();
            var node = new Node { Name = "Node1" };
            builder.AddNode(node);

            var sigInitialValueLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" HEX 0 200;", sigInitialValueLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 150;", sigInitialValueLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_ ""AttributeName"" BU_ Node1 40;", sigInitialValueLineParser, builder, nextLineProvider));
        }

        [Test]
        public void EnumDefinitionCustomPropertyIsParsed()
        {
            var builder = new DbcBuilder();
  
            var sigInitialValueLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" ENUM ""Ciao"",""Mamma"",""Guarda"";", sigInitialValueLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" ""Guarda"";", sigInitialValueLineParser, builder, nextLineProvider));
        }
    }
}