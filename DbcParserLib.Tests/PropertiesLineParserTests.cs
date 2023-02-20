using NUnit.Framework;
using DbcParserLib.Parsers;
using DbcParserLib.Model;
using Moq;
using System.Linq;
using System.IO;

namespace DbcParserLib.Tests
{
    public class PropertiesLineParserTests
    {
        private static ILineParser CreateParser()
        {
            return new PropertiesLineParser();
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
            Assert.IsTrue(msgCycleTimeLineParser.TryParse(@"BA_ ""GenMsgCycleTime"" BO_ 2394947585 100;", builder, nextLineProvider));

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
            Assert.IsTrue(sigInitialValueLineParser.TryParse(@"BA_ ""GenSigStartValue"" SG_ 2394947585 sig_name 40;", builder, nextLineProvider));

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
            Assert.IsTrue(msgCycleTimeLineParser.TryParse(@"BA_DEF_ BO_ ""GenMsgCycleTime"" INT 0 200;", builder, nextLineProvider));
            Assert.IsTrue(msgCycleTimeLineParser.TryParse(@"BA_DEF_DEF_ ""GenMsgCycleTime"" 150;", builder, nextLineProvider));
            Assert.IsTrue(msgCycleTimeLineParser.TryParse(@"BA_ ""GenMsgCycleTime"" BO_ 2394947585 100;", builder, nextLineProvider));

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
            Assert.IsTrue(sigInitialValueLineParser.TryParse(@"BA_DEF_ SG_ ""GenSigStartValue"" INT 0 200;", builder, nextLineProvider));
            Assert.IsTrue(sigInitialValueLineParser.TryParse(@"BA_DEF_DEF_ ""GenSigStartValue"" 150;", builder, nextLineProvider));
            Assert.IsTrue(sigInitialValueLineParser.TryParse(@"BA_ ""GenSigStartValue"" SG_ 2394947585 sig_name 40;", builder, nextLineProvider));

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
            Assert.IsTrue(sigInitialValueLineParser.TryParse(@"BA_DEF_ BU_ ""AttributeName"" HEX 0 200;", builder, nextLineProvider));
            Assert.IsTrue(sigInitialValueLineParser.TryParse(@"BA_DEF_DEF_ ""AttributeName"" 150;", builder, nextLineProvider));
            Assert.IsTrue(sigInitialValueLineParser.TryParse(@"BA_ ""AttributeName"" BU_ Node1 40;", builder, nextLineProvider));
        }
    }
}