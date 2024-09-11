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
            Assert.Multiple(() =>
            {
                Assert.That(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" INT 5 10;", customPropertyLineParsers, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 7;", customPropertyLineParsers, builder, nextLineProvider), Is.True);
            });
        }

        [Test]
        public void IntDefinitionCustomPropertyNoBoundariesIsParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 2394947585 };
            builder.AddMessage(message);

            var msgCycleTimeLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.Multiple(() =>
            {
                Assert.That(ParseLine(@"BA_DEF_ BO_ ""AttributeName"" INT 0 0;", msgCycleTimeLineParser, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 150;", msgCycleTimeLineParser, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_ ""AttributeName"" BO_ 2394947585 100;", msgCycleTimeLineParser, builder, nextLineProvider), Is.True);
            });

            var dbc = builder.Build();
            Assert.Multiple(() =>
            {
                Assert.That(dbc.Messages.First().CustomProperties.TryGetValue("AttributeName", out var customProperty), Is.EqualTo(true));
                Assert.That(customProperty!.IntegerCustomProperty.Value, Is.EqualTo(100));
            });
        }

        [Test]
        public void HexDefinitionCustomPropertyIsParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());

            var customPropertyLineParsers = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.Multiple(() =>
            {
                Assert.That(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" HEX 5 10;", customPropertyLineParsers, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 7;", customPropertyLineParsers, builder, nextLineProvider), Is.True);
            });
        }

        [Test]
        public void HexDefinitionCustomPropertyNoBoundariesIsParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 2394947585 };
            builder.AddMessage(message);

            var msgCycleTimeLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.Multiple(() =>
            {
                Assert.That(ParseLine(@"BA_DEF_ BO_ ""AttributeName"" HEX 0 0;", msgCycleTimeLineParser, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 150;", msgCycleTimeLineParser, builder, nextLineProvider), Is.True);
            });

            var dbc = builder.Build();
            Assert.Multiple(() =>
            {
                Assert.That(dbc.Messages.First().CustomProperties.TryGetValue("AttributeName", out var customProperty), Is.EqualTo(true));
                Assert.That(customProperty!.HexCustomProperty.Value, Is.EqualTo(150));
            });
        }

        [Test]
        public void FloatDefinitionCustomPropertyIsParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());

            var customPropertyLineParsers = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.Multiple(() =>
            {
                Assert.That(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" FLOAT 5 10.5;", customPropertyLineParsers, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 7.5;", customPropertyLineParsers, builder, nextLineProvider), Is.True);
            });
        }

        [Test]
        public void FloatDefinitionCustomPropertyNoBoundariesIsParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 2394947585 };
            builder.AddMessage(message);

            var msgCycleTimeLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.Multiple(() =>
            {
                Assert.That(ParseLine(@"BA_DEF_ BO_ ""AttributeName"" FLOAT 0 0;", msgCycleTimeLineParser, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 150.0;", msgCycleTimeLineParser, builder, nextLineProvider), Is.True);
            });

            var dbc = builder.Build();
            Assert.Multiple(() =>
            {
                Assert.That(dbc.Messages.First().CustomProperties.TryGetValue("AttributeName", out var customProperty), Is.EqualTo(true));
                Assert.That(customProperty!.FloatCustomProperty.Value, Is.EqualTo(150));
            });
        }

        [Test]
        public void ScientificNotationIntegerDefinitionCustomPropertyIsParsedTest()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            dbcBuilderMock.Setup(mock => mock.AddCustomProperty(It.IsAny<CustomPropertyObjectType>(), It.IsAny<CustomPropertyDefinition>()))
                .Callback<CustomPropertyObjectType, CustomPropertyDefinition>((_, customProperty) =>
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(customProperty.Name, Is.EqualTo("AttributeName"));
                        Assert.That(customProperty.DataType, Is.EqualTo(CustomPropertyDataType.Integer));
                        Assert.That(customProperty.IntegerCustomProperty.Minimum, Is.EqualTo(-1000));
                        Assert.That(customProperty.IntegerCustomProperty.Maximum, Is.EqualTo(1000));
                    });
                });

            var customPropertyLineParsers = CreateParser();
            Assert.That(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" INT -1e3 1e+3;", customPropertyLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void ScientificNotationFloatDefinitionCustomPropertyIsParsedTest()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            dbcBuilderMock.Setup(mock => mock.AddCustomProperty(It.IsAny<CustomPropertyObjectType>(), It.IsAny<CustomPropertyDefinition>()))
                .Callback<CustomPropertyObjectType, CustomPropertyDefinition>((_, customProperty) =>
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(customProperty.Name, Is.EqualTo("AttributeName"));
                        Assert.That(customProperty.DataType, Is.EqualTo(CustomPropertyDataType.Float));
                        Assert.That(customProperty.FloatCustomProperty.Minimum, Is.EqualTo(0));
                        Assert.That(customProperty.FloatCustomProperty.Maximum, Is.EqualTo(0.1));
                    });
                });

            var customPropertyLineParsers = CreateParser();
            Assert.That(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" FLOAT 0 1e-1;", customPropertyLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void StringDefinitionCustomPropertyIsParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());

            var customPropertyLineParsers = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.Multiple(() =>
            {
                Assert.That(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" STRING;", customPropertyLineParsers, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" ""DefaultString"";", customPropertyLineParsers, builder, nextLineProvider), Is.True);
            });
        }

        [Test]
        public void StringDefinitionCustomPropertyOnEnvironmentVariableIsParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());

            var customPropertyLineParsers = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.Multiple(() =>
            {
                Assert.That(ParseLine(@"BA_DEF_ EV_ ""AttributeName"" STRING;", customPropertyLineParsers, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" ""DefaultString"";", customPropertyLineParsers, builder, nextLineProvider), Is.True);
            });
        }

        [Test]
        public void StringDefinitionCustomPropertyAsGlobalIsParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());

            var customPropertyLineParsers = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.Multiple(() =>
            {
                Assert.That(ParseLine(@"BA_DEF_ ""AttributeName"" STRING;", customPropertyLineParsers, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" ""DefaultString"";", customPropertyLineParsers, builder, nextLineProvider), Is.True);
            });

            var dbc = builder.Build();

            var globalProperty = dbc.GlobalProperties.FirstOrDefault(x => x.CustomPropertyDefinition.Name.Equals("AttributeName"));
            Assert.That(globalProperty, Is.Not.Null);
            Assert.That(globalProperty.StringCustomProperty, Is.Not.Null);
            Assert.That(globalProperty.StringCustomProperty.Value, Is.EqualTo("DefaultString"));
        }

        [Test]
        public void EnumDefinitionCustomPropertyIsParsedTest()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            dbcBuilderMock.Setup(mock => mock.AddCustomProperty(It.IsAny<CustomPropertyObjectType>(), It.IsAny<CustomPropertyDefinition>()))
                .Callback<CustomPropertyObjectType, CustomPropertyDefinition>((objectType, customProperty) =>
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(customProperty.Name, Is.EqualTo("AttributeName"));
                        Assert.That(customProperty.DataType, Is.EqualTo(CustomPropertyDataType.Enum));
                        Assert.That(customProperty.EnumCustomProperty.Values, Has.Length.EqualTo(3));
                    });
                    Assert.Multiple(() =>
                    {
                        Assert.That(customProperty.EnumCustomProperty.Values[0], Is.EqualTo("Val1"));
                        Assert.That(customProperty.EnumCustomProperty.Values[1], Is.EqualTo("Val2"));
                        Assert.That(customProperty.EnumCustomProperty.Values[2], Is.EqualTo("Val3"));
                    });
                });

            dbcBuilderMock.Setup(mock => mock.AddCustomPropertyDefaultValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Callback<string, string, bool>((propertyName, value, isNumeric) =>
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(propertyName, Is.EqualTo("AttributeName"));
                        Assert.That(value, Is.EqualTo("Val2"));
                        Assert.That(isNumeric, Is.EqualTo(false));
                    });
                });

            var customPropertyLineParsers = CreateParser();
            Assert.Multiple(() =>
            {
                Assert.That(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" ENUM ""Val1"",""Val2"",""Val3"";", customPropertyLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
                Assert.That(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" ""Val2"";", customPropertyLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
            });
        }

        [Test]
        public void EnumDefinitionCustomPropertyMoreWhiteSpaceIsParsedTest()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            dbcBuilderMock.Setup(mock => mock.AddCustomProperty(It.IsAny<CustomPropertyObjectType>(), It.IsAny<CustomPropertyDefinition>()))
                .Callback<CustomPropertyObjectType, CustomPropertyDefinition>((_, customProperty) =>
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(customProperty.Name, Is.EqualTo("AttributeName"));
                        Assert.That(customProperty.DataType, Is.EqualTo(CustomPropertyDataType.Enum));
                        Assert.That(customProperty.EnumCustomProperty.Values, Has.Length.EqualTo(3));
                    });
                    Assert.Multiple(() =>
                    {
                        Assert.That(customProperty.EnumCustomProperty.Values[0], Is.EqualTo("Val1"));
                        Assert.That(customProperty.EnumCustomProperty.Values[1], Is.EqualTo("Val2"));
                        Assert.That(customProperty.EnumCustomProperty.Values[2], Is.EqualTo("Val3"));
                    });
                });

            dbcBuilderMock.Setup(mock => mock.AddCustomPropertyDefaultValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Callback<string, string, bool>((propertyName, value, isNumeric) =>
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(propertyName, Is.EqualTo("AttributeName"));
                        Assert.That(value, Is.EqualTo("Val2"));
                        Assert.That(isNumeric, Is.EqualTo(false));
                    });
                });

            var customPropertyLineParsers = CreateParser();
            Assert.Multiple(() =>
            {
                Assert.That(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" ENUM   ""Val1"",""Val2"",""Val3"" ;", customPropertyLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
                Assert.That(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" ""Val2"";", customPropertyLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
            });
        }

        [Test]
        public void EnumDefinitionCustomPropertyWithWhiteSpaceBetweenEntriesIsParsedTest()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            dbcBuilderMock.Setup(mock => mock.AddCustomProperty(It.IsAny<CustomPropertyObjectType>(), It.IsAny<CustomPropertyDefinition>()))
                .Callback<CustomPropertyObjectType, CustomPropertyDefinition>((_, customProperty) =>
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(customProperty.Name, Is.EqualTo("AttributeName"));
                        Assert.That(customProperty.DataType, Is.EqualTo(CustomPropertyDataType.Enum));
                        Assert.That(customProperty.EnumCustomProperty.Values, Has.Length.EqualTo(4));
                    });
                    Assert.Multiple(() =>
                    {
                        Assert.That(customProperty.EnumCustomProperty.Values[0], Is.EqualTo("Val1"));
                        Assert.That(customProperty.EnumCustomProperty.Values[1], Is.EqualTo("Val2"));
                        Assert.That(customProperty.EnumCustomProperty.Values[2], Is.EqualTo("Val3"));
                        Assert.That(customProperty.EnumCustomProperty.Values[3], Is.EqualTo("Val4"));
                    });
                });

            dbcBuilderMock.Setup(mock => mock.AddCustomPropertyDefaultValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Callback<string, string, bool>((propertyName, value, isNumeric) =>
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(propertyName, Is.EqualTo("AttributeName"));
                        Assert.That(value, Is.EqualTo("Val2"));
                        Assert.That(isNumeric, Is.EqualTo(false));
                    });
                });

            var customPropertyLineParsers = CreateParser();
            Assert.Multiple(() =>
            {
                Assert.That(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" ENUM  ""Val1"",  ""Val2"",      ""Val3"",""Val4"" ;", customPropertyLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
                Assert.That(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" ""Val2"";", customPropertyLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
            });
        }

        [Test]
        public void MsgCycleTimePropertyIsParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 2394947585 };
            builder.AddMessage(message);

            var msgCycleTimeLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.Multiple(() =>
            {
                Assert.That(ParseLine(@"BA_DEF_ BO_ ""GenMsgCycleTime"" INT 0 0;", msgCycleTimeLineParser, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_DEF_DEF_ ""GenMsgCycleTime"" 150;", msgCycleTimeLineParser, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_ ""GenMsgCycleTime"" BO_ 2394947585 100;", msgCycleTimeLineParser, builder, nextLineProvider), Is.True);
            });

            var dbc = builder.Build();
            Assert.Multiple(() =>
            {
                Assert.That(dbc.Messages.First().CycleTime(out var cycleTime), Is.EqualTo(true));
                Assert.That(cycleTime, Is.EqualTo(100));
            });
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
            Assert.Multiple(() =>
            {
                Assert.That(ParseLine(@"BA_DEF_ SG_ ""GenSigStartValue"" INT 0 200;", sigInitialValueLineParser, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_DEF_DEF_ ""GenSigStartValue"" 150;", sigInitialValueLineParser, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_ ""GenSigStartValue"" SG_ 2394947585 sig_name 40;", sigInitialValueLineParser, builder, nextLineProvider), Is.True);
            });

            var dbc = builder.Build();
            Assert.Multiple(() =>
            {
                Assert.That(dbc.Messages.First().Signals.First().InitialValue(out var initialValue), Is.EqualTo(true));
                Assert.That(dbc.Messages.First().Signals.First().InitialValue, Is.EqualTo(40));
                Assert.That(initialValue, Is.EqualTo(40));
            });
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
            Assert.Multiple(() =>
            {
                Assert.That(ParseLine(@"BA_DEF_ SG_ ""GenSigStartValue"" HEX 0 200;", sigInitialValueLineParser, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_DEF_DEF_ ""GenSigStartValue"" 150;", sigInitialValueLineParser, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_ ""GenSigStartValue"" SG_ 2394947585 sig_name 40;", sigInitialValueLineParser, builder, nextLineProvider), Is.True);
            });

            var dbc = builder.Build();
            Assert.Multiple(() =>
            {
                Assert.That(dbc.Messages.First().Signals.First().InitialValue(out var initialValue), Is.EqualTo(true));
                Assert.That(dbc.Messages.First().Signals.First().InitialValue, Is.EqualTo(40));
                Assert.That(initialValue, Is.EqualTo(40));
            });
        }

        [Test]
        public void NodeCustomPropertyIsParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var node = new Node { Name = "Node1" };
            builder.AddNode(node);

            var customPropertyLineParsers = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.Multiple(() =>
            {
                Assert.That(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" HEX 0 200;", customPropertyLineParsers, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 150;", customPropertyLineParsers, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_ ""AttributeName"" BU_ Node1 40;", customPropertyLineParsers, builder, nextLineProvider), Is.True);
            });

            var dbc = builder.Build();
            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes.First().CustomProperties.First().Value.CustomPropertyDefinition.HexCustomProperty.Default, Is.EqualTo(150));
                Assert.That(dbc.Nodes.First().CustomProperties.First().Value.HexCustomProperty.Value, Is.EqualTo(40));
            });
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
            Assert.Multiple(() =>
            {
                Assert.That(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" FLOAT 0 10;", customPropertyLineParsers, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 5;", customPropertyLineParsers, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_ ""AttributeName"" BU_ Node1 0.7e1;", customPropertyLineParsers, builder, nextLineProvider), Is.True);
                Assert.That(dbc.Nodes.First().CustomProperties.First().Value.FloatCustomProperty.Value, Is.EqualTo(7));
            });
        }

        [Test]
        public void NodeMultipleCustomPropertyAreParsedTest()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var node = new Node { Name = "Node1" };
            builder.AddNode(node);

            var customPropertyLineParsers = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));
            Assert.Multiple(() =>
            {
                Assert.That(ParseLine(@"BA_DEF_ BU_ ""AttributeName1"" INT 0 200;", customPropertyLineParsers, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_DEF_DEF_ ""AttributeName1"" 100;", customPropertyLineParsers, builder, nextLineProvider), Is.True);

                Assert.That(ParseLine(@"BA_DEF_ BU_ ""AttributeName2""  FLOAT 0 10;", customPropertyLineParsers, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_DEF_DEF_ ""AttributeName2"" 5.5;", customPropertyLineParsers, builder, nextLineProvider), Is.True);

                Assert.That(ParseLine(@"BA_ ""AttributeName1"" BU_ Node1 40;", customPropertyLineParsers, builder, nextLineProvider), Is.True);
            });

            var dbc = builder.Build();
            Assert.That(dbc.Nodes.First().CustomProperties, Has.Count.EqualTo(2));
            Assert.That(dbc.Nodes.First().CustomProperties["AttributeName2"].FloatCustomProperty.Value, Is.EqualTo(5.5));
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
            Assert.Multiple(() =>
            {
                Assert.That(ParseLine(@"BA_DEF_ BU_ ""AttributeName"" INT 0 200;", customPropertyLineParsers, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_DEF_DEF_ ""AttributeName"" 100;", customPropertyLineParsers, builder, nextLineProvider), Is.True);

                Assert.That(ParseLine(@"BA_ ""AttributeName"" BU_ Node1 40;", customPropertyLineParsers, builder, nextLineProvider), Is.True);
                Assert.That(ParseLine(@"BA_ ""AttributeName"" BU_ Node2 70;", customPropertyLineParsers, builder, nextLineProvider), Is.True);
            });

            var dbc = builder.Build();
            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes.First().CustomProperties["AttributeName"].IntegerCustomProperty.Value, Is.EqualTo(40));
                Assert.That(dbc.Nodes.ElementAt(1).CustomProperties["AttributeName"].IntegerCustomProperty.Value, Is.EqualTo(70));
            });
        }
    }
}