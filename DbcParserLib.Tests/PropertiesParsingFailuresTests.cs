using NUnit.Framework;
using DbcParserLib.Parsers;
using DbcParserLib.Model;
using Moq;
using System.Linq;
using DbcParserLib.Observers;

namespace DbcParserLib.Tests
{
    public class PropertiesParsingFailuresTests
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

        private void ParseLine(string line1, string line2, uint messageId, IParseFailureObserver observer)
        {
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();
            var dbcBuilder = new DbcBuilder(observer);
            dbcBuilder.AddMessage(new Message()
            {
                ID = messageId
            });

            var definitionLineParser = new PropertiesDefinitionLineParser(observer);
            var lineParser = new PropertiesLineParser(observer);
            definitionLineParser.TryParse(line1, dbcBuilder, nextLineProviderMock.Object);
            lineParser.TryParse(line2, dbcBuilder, nextLineProviderMock.Object);
        }

        [TestCase("BA_ \"attributeName\" BO_ -123 100;")]
        [TestCase("BA_ \"attributeName\" SG_ 123 signalName value;")]
        [TestCase("BA_ \"attributeName\" EV_ varName 1e10")]
        [TestCase("BA_ attributeName BU_ nodeName 0;")]
        public void PropertySyntaxErrorIsObserved(string line)
        {
            var observerMock = m_repository.Create<IParseFailureObserver>();
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            observerMock.Setup(o => o.PropertySyntaxError());

            var lineParser = new PropertiesLineParser(observerMock.Object);
            lineParser.TryParse(line, dbcBuilderMock.Object, nextLineProviderMock.Object);
        }

        [Test]
        public void CustomPropertyNotFoundErrorIsObserved()
        {
            var propertyName = "attributeName";
            var nodeName = "nodeName";
            var value = "100";
            var line = $"BA_ \"{propertyName}\" BU_ {nodeName} {value};";

            var observerMock = m_repository.Create<IParseFailureObserver>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();
            var dbcBuilder = new DbcBuilder(observerMock.Object);

            observerMock.Setup(o => o.PropertyNameNotFound(propertyName));

            var lineParser = new PropertiesLineParser(observerMock.Object);
            lineParser.TryParse(line, dbcBuilder, nextLineProviderMock.Object);
        }

        [Test]
        public void CustomPropertyNodeNotFoundErrorIsObserved()
        {
            var propertyName = "attributeName";
            var nodeName = "nodeName";
            var value = "100";

            var line1 = $"BA_DEF_ BU_ \"{propertyName}\" INT 0 65535;";
            var line2 = $"BA_ \"{propertyName}\" BU_ {nodeName} {value};";

            var observerMock = m_repository.Create<IParseFailureObserver>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();
            var dbcBuilder = new DbcBuilder(observerMock.Object);

            observerMock.Setup(o => o.NodeNameNotFound(nodeName));

            ILineParser lineParser = new PropertiesDefinitionLineParser(observerMock.Object);
            lineParser.TryParse(line1, dbcBuilder, nextLineProviderMock.Object);
            
            lineParser = new PropertiesLineParser(observerMock.Object);
            lineParser.TryParse(line2, dbcBuilder, nextLineProviderMock.Object);
        }

        [Test]
        public void DuplicateCustomPropertyErrorIsObserved()
        {
            var propertyName = "attributeName";
            var nodeName = "nodeName";
            var value = "100";

            var line1 = $"BA_DEF_ BU_ \"{propertyName}\" INT 0 65535;";
            var line2 = $"BA_ \"{propertyName}\" BU_ {nodeName} {value};";

            var observerMock = m_repository.Create<IParseFailureObserver>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();
            var dbcBuilder = new DbcBuilder(observerMock.Object);

            observerMock.Setup(o => o.DuplicatedPropertyInNode(propertyName, nodeName));

            dbcBuilder.AddNode(new Node()
            {
                Name = nodeName
            });
            ILineParser lineParser = new PropertiesDefinitionLineParser(observerMock.Object);
            lineParser.TryParse(line1, dbcBuilder, nextLineProviderMock.Object);
            
            lineParser = new PropertiesLineParser(observerMock.Object);
            lineParser.TryParse(line2, dbcBuilder, nextLineProviderMock.Object);
            lineParser.TryParse(line2, dbcBuilder, nextLineProviderMock.Object);
        }

        [TestCase("BA_DEF_ BO_ \"GenMsgCycleTime\" INT 1.5 65535;")]
        [TestCase("BA_DEF_ \"attributeName\" STRING")]
        [TestCase("BA_DEF_ SGG_ \"attributeName\" FLOAT -3.4E+038 3.4E+038;")]
        [TestCase("BA_DEF_ BU_ \"attributeName\" STRING 0;")]
        [TestCase("BA_DEF_ attributeName STRING")]
        [TestCase("BA_DEF_ BU_ \"Ciao\" ENUM  \"Cyclic\"\"Event\",\"CyclicIfActive\",\"SpontanWithDelay\",\"CyclicAndSpontan\";")]
        [TestCase("BA_DEF_ BU_ \"Ciao\" ENUM  \"Cyclic\",\"Event\", \"CyclicIfActive\",\"SpontanWithDelay\",\"CyclicAndSpontan\";")]
        public void PropertyDefinitionSyntaxErrorIsObserved(string line)
        {
            var observerMock = m_repository.Create<IParseFailureObserver>();
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            observerMock.Setup(o => o.PropertyDefinitionSyntaxError());

            var lineParser = new PropertiesDefinitionLineParser(observerMock.Object);
            lineParser.TryParse(line, dbcBuilderMock.Object, nextLineProviderMock.Object);
        }

        [Test]
        public void CustomPropertyDuplicateErrorIsObserved()
        {
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" STRING;";
            var line2 = $"BA_DEF_ BO_ \"{propertyName}\" INT 0 65535;";

            var observerMock = m_repository.Create<IParseFailureObserver>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();
            var dbcBuilder = new DbcBuilder(observerMock.Object);

            observerMock.Setup(o => o.DuplicatedProperty(propertyName));

            var lineParser = new PropertiesDefinitionLineParser(observerMock.Object);
            lineParser.TryParse(line1, dbcBuilder, nextLineProviderMock.Object);
            lineParser.TryParse(line2, dbcBuilder, nextLineProviderMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyIntegerWithStringValueErrorIsObserved()
        {
            uint messageId = 123456;
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" INT 0 100;";
            var line2 = $"BA_ \"{propertyName}\" BO_ {messageId} \"50\";";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertySyntaxError());
            ParseLine(line1, line2, messageId, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyHexWithStringValueErrorIsObserved()
        {
            uint messageId = 123456;
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" HEX 0 100;";
            var line2 = $"BA_ \"{propertyName}\" BO_ {messageId} \"50\";";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertySyntaxError());
            ParseLine(line1, line2, messageId, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyFloatWithStringValueErrorIsObserved()
        {
            uint messageId = 123456;
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" FLOAT 0 100;";
            var line2 = $"BA_ \"{propertyName}\" BO_ {messageId} \"50\";";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertySyntaxError());
            ParseLine(line1, line2, messageId, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyIntegerOutOfBoundErrorIsObserved()
        {
            uint messageId = 123456;
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" INT 0 100;";
            var line2 = $"BA_ \"{propertyName}\" BO_ {messageId} 110;";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertyValueOutOfBound(propertyName, "110"));
            ParseLine(line1, line2, messageId, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyHexOutOfBoundErrorIsObserved()
        {
            uint messageId = 123456;
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" HEX 0 100;";
            var line2 = $"BA_ \"{propertyName}\" BO_ {messageId} 110;";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertyValueOutOfBound(propertyName, "110"));
            ParseLine(line1, line2, messageId, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyFloatOutOfBoundErrorIsObserved()
        {
            uint messageId = 123456;
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" FLOAT 0 100;";
            var line2 = $"BA_ \"{propertyName}\" BO_ {messageId} 110;";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertyValueOutOfBound(propertyName, "110"));
            ParseLine(line1, line2, messageId, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyIntegerNotIntegerValueErrorIsObserved()
        {
            uint messageId = 123456;
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" INT 0 100;";
            var line2 = $"BA_ \"{propertyName}\" BO_ {messageId} 1.5;";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertySyntaxError());
            ParseLine(line1, line2, messageId, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyHexNotIntegerValueErrorIsObserved()
        {
            uint messageId = 123456;
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" HEX 0 100;";
            var line2 = $"BA_ \"{propertyName}\" BO_ {messageId} 1.5;";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertySyntaxError());
            ParseLine(line1, line2, messageId, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyStringNotStringValueErrorIsObserved()
        {
            uint messageId = 123456;
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" STRING;";
            var line2 = $"BA_ \"{propertyName}\" BO_ {messageId} 10;";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertySyntaxError());
            ParseLine(line1, line2, messageId, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyEnumNotIntegerIndexValueErrorIsObserved()
        {
            uint messageId = 123456;
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" ENUM \"FirstVal\",\"SecondVal\",\"ThirdVal\";";
            var line2 = $"BA_ \"{propertyName}\" BO_ {messageId} 1.5;";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertySyntaxError());
            ParseLine(line1, line2, messageId, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyEnumOutOfIndexValueErrorIsObserved()
        {
            uint messageId = 123456;
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" ENUM \"FirstVal\",\"SecondVal\",\"ThirdVal\";";
            var line2 = $"BA_ \"{propertyName}\" BO_ {messageId} 3;";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertyValueOutOfIndex(propertyName, "3"));
            ParseLine(line1, line2, messageId, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyEnumOutOfBoundValueErrorIsObserved()
        {
            uint messageId = 123456;
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" ENUM \"FirstVal\",\"SecondVal\",\"ThirdVal\";";
            var line2 = $"BA_ \"{propertyName}\" BO_ {messageId} \"3\";";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertyValueOutOfBound(propertyName, "3"));
            ParseLine(line1, line2, messageId, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyEnumWithIntegerValuesIsParsed()
        {
            var dbcString = @"
BO_ 200 SENSOR: 39 SENSOR
 BA_DEF_ BO_ ""AttributeName"" ENUM ""1"",""2"",""3"";
 BA_ ""AttributeName"" BO_ 200 ""3"";";

            var dbc = Parser.Parse(dbcString);

            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual("3", dbc.Messages.First().CustomProperties.Values.First().EnumCustomProperty.Value);
        }

        [Test]
        public void DefaultCustomPropertyEnumWithIntegerValuesByIndexIsParsed()
        {
            var dbcString = @"
BO_ 200 SENSOR: 39 SENSOR
 BA_DEF_ BO_ ""AttributeName"" ENUM ""1"",""2"",""3"";
 BA_ ""AttributeName"" BO_ 200 2;";

            var dbc = Parser.Parse(dbcString);

            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual("3", dbc.Messages.First().CustomProperties.Values.First().EnumCustomProperty.Value);
        }
    }
}