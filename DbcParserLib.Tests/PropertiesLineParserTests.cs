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
        public void IntDefinitionCustomPropertyIsParsedTest()
        {
            var builder = new DbcBuilder();

            var sigInitialValueLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" INT 5 10;", sigInitialValueLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 7;", sigInitialValueLineParser, builder, nextLineProvider));
        }

        [Test]
        public void FloatDefinitionCustomPropertyIsParsedTest()
        {
            var builder = new DbcBuilder();

            var sigInitialValueLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" FLOAT 5 10.5;", sigInitialValueLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 7.5;", sigInitialValueLineParser, builder, nextLineProvider));
        }

        [Test]
        public void StringDefinitionCustomPropertyIsParsedTest()
        {
            var builder = new DbcBuilder();

            var sigInitialValueLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" STRING;", sigInitialValueLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" ""DefaultString"";", sigInitialValueLineParser, builder, nextLineProvider));
        }

        [Test]
        public void EnumDefinitionCustomPropertyIsParsedTest()
        {
            var builder = new DbcBuilder();

            var sigInitialValueLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" ENUM ""Val1"",""Val2"",""Val3"";", sigInitialValueLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" ""Val2"";", sigInitialValueLineParser, builder, nextLineProvider));
        }

        [Test]
        public void MsgCustomPropertyIsParsedTest()
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
        public void SigCustomPropertyIsParsedTest()
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
        public void NodeCustomPropertyIsParsedTest()
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
        public void NodeMultipleCustomPropertyAreParsedTest()
        {
            var builder = new DbcBuilder();
            var node = new Node { Name = "Node1" };
            builder.AddNode(node);

            var sigInitialValueLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName1"" INT 0 200;", sigInitialValueLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName1"" 100;", sigInitialValueLineParser, builder, nextLineProvider));

            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName2""  FLOAT 0 10;", sigInitialValueLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName2"" 5.5;", sigInitialValueLineParser, builder, nextLineProvider));

            Assert.IsTrue(ParseLine(@"BA_ ""AttributeName1"" BU_ Node1 40;", sigInitialValueLineParser, builder, nextLineProvider));

            var dbc = builder.Build();
            Assert.AreEqual(2, dbc.Nodes.First().CustomProperties.Count);
            Assert.AreEqual(5.5, dbc.Nodes.First().CustomProperties["AttributeName2"].FloatCustomProperty.Value);
        }

        [Test]
        public void CustomPropertyIsAssignedToDifferentNodesTest()
        {
            var builder = new DbcBuilder();
            var node1 = new Node { Name = "Node1" };
            var node2 = new Node { Name = "Node2" };
            builder.AddNode(node1);
            builder.AddNode(node2);

            var sigInitialValueLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" INT 0 200;", sigInitialValueLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 100;", sigInitialValueLineParser, builder, nextLineProvider));

            Assert.IsTrue(ParseLine(@"BA_ ""AttributeName"" BU_ Node1 40;", sigInitialValueLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_ ""AttributeName"" BU_ Node2 70;", sigInitialValueLineParser, builder, nextLineProvider));

            var dbc = builder.Build();
            Assert.AreEqual(40, dbc.Nodes.First().CustomProperties["AttributeName"].IntegerCustomProperty.Value);
            Assert.AreEqual(70, dbc.Nodes.ElementAt(1).CustomProperties["AttributeName"].IntegerCustomProperty.Value);
        }
    }
}