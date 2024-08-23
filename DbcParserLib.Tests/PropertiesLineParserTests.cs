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
            Assert.That(dbc.Messages.First().Value.CustomProperties.TryGetValue("AttributeName", out var customProperty));
            Assert.That(customProperty.PropertyValue is IntegerPropertyValue);
            Assert.AreEqual(100, ((IntegerPropertyValue)customProperty.PropertyValue).Value);
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
            Assert.That(dbc.Messages.First().Value.CustomProperties.TryGetValue("AttributeName", out var customProperty));
            Assert.That(customProperty.PropertyValue is HexPropertyValue);
            Assert.AreEqual(150, ((HexPropertyValue)customProperty.PropertyValue).Value);
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
            Assert.That(dbc.Messages.First().Value.CustomProperties.TryGetValue("AttributeName", out var customProperty));
            Assert.That(customProperty.PropertyValue is FloatPropertyValue);
            Assert.AreEqual(150, ((FloatPropertyValue)customProperty.PropertyValue).Value);
        }

        [Test]
        public void ScientificNotationDefinitionCustomPropertyIsParsedTest()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            dbcBuilderMock.Setup(mock => mock.AddCustomProperty(It.IsAny<CustomPropertyObjectType>(), It.IsAny<CustomProperty>()))
                .Callback<CustomPropertyObjectType, CustomProperty>((_, customProperty) =>
                {
                    Assert.AreEqual("AttributeName", customProperty.Name);
                    Assert.AreEqual(CustomPropertyDataType.Float, customProperty.DataType);
                    Assert.That(customProperty.PropertyDefinition is FloatCustomPropertyDefinition);
                    var floatCustomProperty = (FloatCustomPropertyDefinition)customProperty.PropertyDefinition;
                    Assert.AreEqual(0, floatCustomProperty.Minimum);
                    Assert.AreEqual(0.1, floatCustomProperty.Maximum);
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

            dbcBuilderMock.Setup(mock => mock.AddCustomProperty(It.IsAny<CustomPropertyObjectType>(), It.IsAny<CustomProperty>()))
                .Callback<CustomPropertyObjectType, CustomProperty>((objectType, customProperty) =>
                {
                    Assert.AreEqual("AttributeName", customProperty.Name);
                    Assert.AreEqual(CustomPropertyDataType.Enum, customProperty.DataType);
                    Assert.That(customProperty.PropertyDefinition is EnumCustomPropertyDefinition);
                    var enumCustomProperty = (EnumCustomPropertyDefinition)customProperty.PropertyDefinition;
                    Assert.AreEqual(3, enumCustomProperty.Values.Count);
                    Assert.AreEqual("Val1", enumCustomProperty.Values.ElementAt(0));
                    Assert.AreEqual("Val2", enumCustomProperty.Values.ElementAt(1));
                    Assert.AreEqual("Val3", enumCustomProperty.Values.ElementAt(2));
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

            dbcBuilderMock.Setup(mock => mock.AddCustomProperty(It.IsAny<CustomPropertyObjectType>(), It.IsAny<CustomProperty>()))
                .Callback<CustomPropertyObjectType, CustomProperty>((_, customProperty) =>
                {
                    Assert.AreEqual("AttributeName", customProperty.Name);
                    Assert.AreEqual(CustomPropertyDataType.Enum, customProperty.DataType);
                    Assert.That(customProperty.PropertyDefinition is EnumCustomPropertyDefinition);
                    var enumCustomProperty = (EnumCustomPropertyDefinition)customProperty.PropertyDefinition;
                    Assert.AreEqual(3, enumCustomProperty.Values.Count);
                    Assert.AreEqual("Val1", enumCustomProperty.Values.ElementAt(0));
                    Assert.AreEqual("Val2", enumCustomProperty.Values.ElementAt(1));
                    Assert.AreEqual("Val3", enumCustomProperty.Values.ElementAt(2));
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
            Assert.IsNotNull(dbc.Messages.First().Value.CycleTime);
            Assert.AreEqual(100, dbc.Messages.First().Value.CycleTime);
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
            Assert.NotNull(dbc.Messages.First().Value.Signals.First().Value.InitialValue);
            Assert.AreEqual(40, dbc.Messages.First().Value.Signals.First().Value.InitialValue);
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
            Assert.NotNull(dbc.Messages.First().Value.Signals.First().Value.InitialValue);
            Assert.AreEqual(40, dbc.Messages.First().Value.Signals.First().Value.InitialValue);
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
            Assert.That(dbc.Nodes.First().CustomProperties.First().Value.PropertyDefinition is HexCustomPropertyDefinition);
            var hexCustomPropertyDefinition = (HexCustomPropertyDefinition)dbc.Nodes.First().CustomProperties.First().Value.PropertyDefinition;
            Assert.AreEqual(150, hexCustomPropertyDefinition.Default);
            Assert.That(dbc.Nodes.First().CustomProperties.First().Value.PropertyValue is HexPropertyValue);
            Assert.AreEqual(40, ((HexPropertyValue)dbc.Nodes.First().CustomProperties.First().Value.PropertyValue).Value);
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
            Assert.That(dbc.Nodes.First().CustomProperties.First().Value.PropertyValue is FloatPropertyValue);
            Assert.AreEqual(7, ((FloatPropertyValue)dbc.Nodes.First().CustomProperties.First().Value.PropertyValue).Value);
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
            Assert.That(dbc.Nodes.First().CustomProperties["AttributeName2"].PropertyValue is FloatPropertyValue);
            Assert.AreEqual(5.5, ((FloatPropertyValue)dbc.Nodes.First().CustomProperties["AttributeName2"].PropertyValue).Value);
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
            Assert.That(dbc.Nodes.First().CustomProperties["AttributeName"].PropertyValue is IntegerPropertyValue);
            Assert.AreEqual(40, ((IntegerPropertyValue)dbc.Nodes.First().CustomProperties["AttributeName"].PropertyValue).Value);
            Assert.That(dbc.Nodes.ElementAt(1).CustomProperties["AttributeName"].PropertyValue is IntegerPropertyValue);
            Assert.AreEqual(70, ((IntegerPropertyValue)dbc.Nodes.ElementAt(1).CustomProperties["AttributeName"].PropertyValue).Value);
        }
    }
}