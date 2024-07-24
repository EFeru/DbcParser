using NUnit.Framework;
using DbcParserLib.Parsers;
using DbcParserLib.Model;
using Moq;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using DbcParserLib.Observers;

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
            var observer = new SilentFailureObserver();
            return new List<ILineParser>() {
                new PropertiesLineParser(observer),
                new PropertiesDefinitionLineParser(observer)
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
            var builder = new DbcBuilder(new SilentFailureObserver());

            var customPropertyLineParsers = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" INT 5 10;", customPropertyLineParsers, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 7;", customPropertyLineParsers, builder, nextLineProvider));
        }

        [Test]
        public void IntDefinitionCustomPropertyNoBoundariesIsParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 2394947585 };
            builder.AddMessage(message);

            var msgCycleTimeLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ BO_ ""AttributeName"" INT 0 0;", msgCycleTimeLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 150;", msgCycleTimeLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_ ""AttributeName"" BO_ 2394947585 100;", msgCycleTimeLineParser, builder, nextLineProvider));

            var dbc = builder.Build();
            Assert.AreEqual(true, dbc.Messages.First().CustomProperties.TryGetValue("AttributeName", out var customProperty));
            Assert.AreEqual(100, customProperty!.IntegerCustomProperty.Value);
        }

        [Test]
        public void HexDefinitionCustomPropertyIsParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());

            var customPropertyLineParsers = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" HEX 5 10;", customPropertyLineParsers, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 7;", customPropertyLineParsers, builder, nextLineProvider));
        }

        [Test]
        public void HexDefinitionCustomPropertyNoBoundariesIsParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 2394947585 };
            builder.AddMessage(message);

            var msgCycleTimeLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ BO_ ""AttributeName"" HEX 0 0;", msgCycleTimeLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 150;", msgCycleTimeLineParser, builder, nextLineProvider));

            var dbc = builder.Build();
            Assert.AreEqual(true, dbc.Messages.First().CustomProperties.TryGetValue("AttributeName", out var customProperty));
            Assert.AreEqual(150, customProperty!.HexCustomProperty.Value);
        }

        [Test]
        public void FloatDefinitionCustomPropertyIsParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());

            var customPropertyLineParsers = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" FLOAT 5 10.5;", customPropertyLineParsers, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 7.5;", customPropertyLineParsers, builder, nextLineProvider));
        }

        [Test]
        public void FloatDefinitionCustomPropertyNoBoundariesIsParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 2394947585 };
            builder.AddMessage(message);

            var msgCycleTimeLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ BO_ ""AttributeName"" FLOAT 0 0;", msgCycleTimeLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 150.0;", msgCycleTimeLineParser, builder, nextLineProvider));

            var dbc = builder.Build();
            Assert.AreEqual(true, dbc.Messages.First().CustomProperties.TryGetValue("AttributeName", out var customProperty));
            Assert.AreEqual(150, customProperty!.FloatCustomProperty.Value);
        }

        [Test]
        public void ScientificNotationDefinitionCustomPropertyIsParsedTest()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            dbcBuilderMock.Setup(mock => mock.AddCustomProperty(It.IsAny<CustomPropertyObjectType>(), It.IsAny<CustomPropertyDefinition>()))
                .Callback<CustomPropertyObjectType, CustomPropertyDefinition>((_, customProperty) =>
                {
                    Assert.AreEqual("AttributeName", customProperty.Name);
                    Assert.AreEqual(CustomPropertyDataType.Float, customProperty.DataType);
                    Assert.AreEqual(0, customProperty.FloatCustomProperty.Minimum);
                    Assert.AreEqual(0.1, customProperty.FloatCustomProperty.Maximum);
                });

            var customPropertyLineParsers = CreateParser();
            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" FLOAT 0 1e-1;", customPropertyLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void StringDefinitionCustomPropertyIsParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());

            var customPropertyLineParsers = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" STRING;", customPropertyLineParsers, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" ""DefaultString"";", customPropertyLineParsers, builder, nextLineProvider));
        }

        [Test]
        public void EnumDefinitionCustomPropertyIsParsedTest()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            dbcBuilderMock.Setup(mock => mock.AddCustomProperty(It.IsAny<CustomPropertyObjectType>(), It.IsAny<CustomPropertyDefinition>()))
                .Callback<CustomPropertyObjectType, CustomPropertyDefinition>((objectType, customProperty) =>
                {
                    Assert.AreEqual("AttributeName", customProperty.Name);
                    Assert.AreEqual(CustomPropertyDataType.Enum, customProperty.DataType);
                    Assert.AreEqual(3, customProperty.EnumCustomProperty.Values.Length);
                    Assert.AreEqual("Val1", customProperty.EnumCustomProperty.Values[0]);
                    Assert.AreEqual("Val2", customProperty.EnumCustomProperty.Values[1]);
                    Assert.AreEqual("Val3", customProperty.EnumCustomProperty.Values[2]);
                });

            dbcBuilderMock.Setup(mock => mock.AddCustomPropertyDefaultValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Callback<string, string, bool>((propertyName, value, isNumeric) =>
                {
                    Assert.AreEqual("AttributeName", propertyName);
                    Assert.AreEqual("Val2", value);
                    Assert.AreEqual(false, isNumeric);
                });

            var customPropertyLineParsers = CreateParser();
            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" ENUM ""Val1"",""Val2"",""Val3"";", customPropertyLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" ""Val2"";", customPropertyLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void EnumDefinitionCustomPropertyMoreWhiteSpaceIsParsedTest()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            dbcBuilderMock.Setup(mock => mock.AddCustomProperty(It.IsAny<CustomPropertyObjectType>(), It.IsAny<CustomPropertyDefinition>()))
                .Callback<CustomPropertyObjectType, CustomPropertyDefinition>((_, customProperty) =>
                {
                    Assert.AreEqual("AttributeName", customProperty.Name);
                    Assert.AreEqual(CustomPropertyDataType.Enum, customProperty.DataType);
                    Assert.AreEqual(3, customProperty.EnumCustomProperty.Values.Length);
                    Assert.AreEqual("Val1", customProperty.EnumCustomProperty.Values[0]);
                    Assert.AreEqual("Val2", customProperty.EnumCustomProperty.Values[1]);
                    Assert.AreEqual("Val3", customProperty.EnumCustomProperty.Values[2]);
                });

            dbcBuilderMock.Setup(mock => mock.AddCustomPropertyDefaultValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Callback<string, string, bool>((propertyName, value, isNumeric) =>
                {
                    Assert.AreEqual("AttributeName", propertyName);
                    Assert.AreEqual("Val2", value);
                    Assert.AreEqual(false, isNumeric);
                });

            var customPropertyLineParsers = CreateParser();
            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" ENUM   ""Val1"",""Val2"",""Val3"" ;", customPropertyLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" ""Val2"";", customPropertyLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void MsgCycleTimePropertyIsParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 2394947585 };
            builder.AddMessage(message);

            var msgCycleTimeLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ BO_ ""GenMsgCycleTime"" INT 0 0;", msgCycleTimeLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""GenMsgCycleTime"" 150;", msgCycleTimeLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_ ""GenMsgCycleTime"" BO_ 2394947585 100;", msgCycleTimeLineParser, builder, nextLineProvider));

            var dbc = builder.Build();
            Assert.AreEqual(true, dbc.Messages.First().CycleTime(out var cycleTime));
            Assert.AreEqual(100, cycleTime);
        }

        [Test]
        public void SigInitialValueIntegerPropertyIsParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 2394947585 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "sig_name" };
            builder.AddSignal(signal);

            var sigInitialValueLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ SG_ ""GenSigStartValue"" INT 0 200;", sigInitialValueLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""GenSigStartValue"" 150;", sigInitialValueLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_ ""GenSigStartValue"" SG_ 2394947585 sig_name 40;", sigInitialValueLineParser, builder, nextLineProvider));

            var dbc = builder.Build();
            Assert.AreEqual(true, dbc.Messages.First().Signals.First().InitialValue(out var initialValue));
            Assert.AreEqual(40, dbc.Messages.First().Signals.First().InitialValue);
            Assert.AreEqual(40, initialValue);
        }

        [Test]
        public void SigInitialValueHexPropertyIsParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 2394947585 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "sig_name" };
            builder.AddSignal(signal);

            var sigInitialValueLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ SG_ ""GenSigStartValue"" HEX 0 200;", sigInitialValueLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""GenSigStartValue"" 150;", sigInitialValueLineParser, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_ ""GenSigStartValue"" SG_ 2394947585 sig_name 40;", sigInitialValueLineParser, builder, nextLineProvider));

            var dbc = builder.Build();
            Assert.AreEqual(true, dbc.Messages.First().Signals.First().InitialValue(out var initialValue));
            Assert.AreEqual(40, dbc.Messages.First().Signals.First().InitialValue);
            Assert.AreEqual(40, initialValue);
        }

        [Test]
        public void NodeCustomPropertyIsParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var node = new Node { Name = "Node1" };
            builder.AddNode(node);

            var customPropertyLineParsers = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" HEX 0 200;", customPropertyLineParsers, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 150;", customPropertyLineParsers, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_ ""AttributeName"" BU_ Node1 40;", customPropertyLineParsers, builder, nextLineProvider));

            var dbc = builder.Build();
            Assert.AreEqual(dbc.Nodes.First().CustomProperties.First().Value.CustomPropertyDefinition.HexCustomProperty.Default, 150);
            Assert.AreEqual(dbc.Nodes.First().CustomProperties.First().Value.HexCustomProperty.Value, 40);
        }

        [Test]
        public void NodeScientificNotationCustomPropertyIsParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var node = new Node { Name = "Node1" };
            builder.AddNode(node);

            var dbc = builder.Build();
            var customPropertyLineParsers = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" FLOAT 0 10;", customPropertyLineParsers, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 5;", customPropertyLineParsers, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_ ""AttributeName"" BU_ Node1 0.7e1;", customPropertyLineParsers, builder, nextLineProvider));
            Assert.AreEqual(dbc.Nodes.First().CustomProperties.First().Value.FloatCustomProperty.Value, 7);
        }

        [Test]
        public void NodeMultipleCustomPropertyAreParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var node = new Node { Name = "Node1" };
            builder.AddNode(node);

            var customPropertyLineParsers = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName1"" INT 0 200;", customPropertyLineParsers, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName1"" 100;", customPropertyLineParsers, builder, nextLineProvider));

            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName2""  FLOAT 0 10;", customPropertyLineParsers, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName2"" 5.5;", customPropertyLineParsers, builder, nextLineProvider));

            Assert.IsTrue(ParseLine(@"BA_ ""AttributeName1"" BU_ Node1 40;", customPropertyLineParsers, builder, nextLineProvider));

            var dbc = builder.Build();
            Assert.AreEqual(2, dbc.Nodes.First().CustomProperties.Count);
            Assert.AreEqual(5.5, dbc.Nodes.First().CustomProperties["AttributeName2"].FloatCustomProperty.Value);
        }

        [Test]
        public void CustomPropertyIsAssignedToDifferentNodesTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var node1 = new Node { Name = "Node1" };
            var node2 = new Node { Name = "Node2" };
            builder.AddNode(node1);
            builder.AddNode(node2);

            var customPropertyLineParsers = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.IsTrue(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" INT 0 200;", customPropertyLineParsers, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 100;", customPropertyLineParsers, builder, nextLineProvider));

            Assert.IsTrue(ParseLine(@"BA_ ""AttributeName"" BU_ Node1 40;", customPropertyLineParsers, builder, nextLineProvider));
            Assert.IsTrue(ParseLine(@"BA_ ""AttributeName"" BU_ Node2 70;", customPropertyLineParsers, builder, nextLineProvider));

            var dbc = builder.Build();
            Assert.AreEqual(40, dbc.Nodes.First().CustomProperties["AttributeName"].IntegerCustomProperty.Value);
            Assert.AreEqual(70, dbc.Nodes.ElementAt(1).CustomProperties["AttributeName"].IntegerCustomProperty.Value);
        }
    }
}