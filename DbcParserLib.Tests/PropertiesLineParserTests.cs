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
        private MockRepository m_repository;

        [SetUp]
        public void Setup()
        {
            m_repository = new MockRepository(MockBehavior.Strict);
        }

        [TearDown]
        public void Teardown()
        {
            m_repository.VerifyAll();
        }

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
        public void ScientificNotationDefinitionCustomPropertyIsParsedTest()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();
 
            dbcBuilderMock.Setup(mock => mock.AddCustomProperty(It.IsAny<DbcObjectType>(), It.IsAny<CustomPropertyDefinition>()))
                .Callback<DbcObjectType, CustomPropertyDefinition>((objectType, customProperty) =>
                {
                    Assert.AreEqual("AttributeName", customProperty.Name);
                    Assert.AreEqual(DbcDataType.Float, customProperty.DataType);
                    Assert.AreEqual(0, customProperty.FloatCustomProperty.Minimum);
                    Assert.AreEqual(0.1, customProperty.FloatCustomProperty.Maximum);
                });

            var sigInitialValueLineParser = CreateParser();
            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" FLOAT 0 1e-1;", sigInitialValueLineParser, dbcBuilderMock.Object, nextLineProviderMock.Object));
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
        public void NodeScientificNotationCustomPropertyIsParsedTest()
        {
            var builder = new DbcBuilder();
            var node = new Node { Name = "Node1" };
            builder.AddNode(node);

            var dbc = builder.Build();
            var sigInitialValueLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" FLOAT 0 10;", sigInitialValueLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 5;", sigInitialValueLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_ ""AttributeName"" BU_ Node1 0.7e1;", sigInitialValueLineParser, builder, nextLineProvider));
            Assert.AreEqual(dbc.Nodes.First().CustomProperties.First().Value.FloatCustomProperty.Value, 7);
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