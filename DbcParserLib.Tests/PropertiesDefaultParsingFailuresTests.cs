using NUnit.Framework;
using DbcParserLib.Parsers;
using Moq;
using System.Linq;
using DbcParserLib.Observers;

namespace DbcParserLib.Tests
{
    public class PropertiesDefaultParsingFailuresTests
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

        private void ParseLine(string line1, string line2, IParseFailureObserver observer)
        {
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();
            var dbcBuilder = new DbcBuilder(observer);

            var lineParser = new PropertiesDefinitionLineParser(observer);
            lineParser.TryParse(line1, dbcBuilder, nextLineProviderMock.Object);
            lineParser.TryParse(line2, dbcBuilder, nextLineProviderMock.Object);
        }

        [TestCase("BA_DEF_DEF_ \"attributeName\" BO_ -123 100;")]
        [TestCase("BA_DEF_DEF_ \"attributeName\" SG_ 123 signalName value")]
        [TestCase("BA_DEF_DEF_ \"attributeName\" EV_ 0varName 1e10;")]
        [TestCase("BA_DEF_DEF_ attributeName BU_ nodeName 0;")]
        public void PropertyDefaultSyntaxErrorIsObserved(string line)
        {
            var observerMock = m_repository.Create<IParseFailureObserver>();
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            observerMock.Setup(o => o.PropertyDefaultSyntaxError());

            var lineParser = new PropertiesDefinitionLineParser(observerMock.Object);
            lineParser.TryParse(line, dbcBuilderMock.Object, nextLineProviderMock.Object);
        }

        [Test]
        public void CustomPropertyDefaultNotFoundErrorIsObserved()
        {
            var propertyName = "attributeName";
            var line = $"BA_DEF_DEF_ \"{propertyName}\" 0;";

            var observerMock = m_repository.Create<IParseFailureObserver>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();
            var dbcBuilder = new DbcBuilder(observerMock.Object);

            observerMock.Setup(o => o.PropertyNameNotFound(propertyName));

            var lineParser = new PropertiesDefinitionLineParser(observerMock.Object);
            lineParser.TryParse(line, dbcBuilder, nextLineProviderMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyIntegerWithStringValueErrorIsObserved()
        {
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" INT 0 100;";
            var line2 = $"BA_DEF_DEF_ \"{propertyName}\" \"50\";";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertySyntaxError());
            ParseLine(line1, line2, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyHexWithStringValueErrorIsObserved()
        {
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" HEX 0 100;";
            var line2 = $"BA_DEF_DEF_ \"{propertyName}\" \"50\";";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertySyntaxError());
            ParseLine(line1, line2, observerMock.Object);

        }

        [Test]
        public void DefaultCustomPropertyFloatWithStringValueErrorIsObserved()
        {
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" FLOAT 0 100;";
            var line2 = $"BA_DEF_DEF_ \"{propertyName}\" \"50\";";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertySyntaxError());
            ParseLine(line1, line2, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyIntegerOutOfBoundErrorIsObserved()
        {
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" INT 0 100;";
            var line2 = $"BA_DEF_DEF_ \"{propertyName}\" 110;";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertyValueOutOfBound(propertyName, "110"));
            ParseLine(line1, line2, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyHexOutOfBoundErrorIsObserved()
        {
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" HEX 0 100;";
            var line2 = $"BA_DEF_DEF_ \"{propertyName}\" 110;";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertyValueOutOfBound(propertyName, "110"));
            ParseLine(line1, line2, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyFloatOutOfBoundErrorIsObserved()
        {
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" FLOAT 0 10;";
            var line2 = $"BA_DEF_DEF_ \"{propertyName}\" 20;";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertyValueOutOfBound(propertyName, "20"));
            ParseLine(line1, line2, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyIntegerNotIntegerValueErrorIsObserved()
        {
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" INT 0 100;";
            var line2 = $"BA_DEF_DEF_ \"{propertyName}\" 1.5;";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertySyntaxError());
            ParseLine(line1, line2, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyHexNotIntegerValueErrorIsObserved()
        {
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" HEX 0 100;";
            var line2 = $"BA_DEF_DEF_ \"{propertyName}\" 1.5;";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertySyntaxError());
            ParseLine(line1, line2, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyStringNotStringValueErrorIsObserved()
        {
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" STRING;";
            var line2 = $"BA_DEF_DEF_ \"{propertyName}\" 10;";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertySyntaxError());
            ParseLine(line1, line2, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyEnumNotIntegerIndexValueErrorIsObserved()
        {
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" ENUM \"FirstVal\",\"SecondVal\",\"ThirdVal\";";
            var line2 = $"BA_DEF_DEF_ \"{propertyName}\" 1.5;";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertySyntaxError());
            ParseLine(line1, line2, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyEnumOutOfIndexValueErrorIsObserved()
        {
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" ENUM \"FirstVal\",\"SecondVal\",\"ThirdVal\";";
            var line2 = $"BA_DEF_DEF_ \"{propertyName}\" 3;";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertyValueOutOfIndex(propertyName, "3"));
            ParseLine(line1, line2, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyEnumOutOfBoundValueErrorIsObserved()
        {
            var propertyName = "AttributeName";
            var line1 = $"BA_DEF_ BO_ \"{propertyName}\" ENUM \"FirstVal\",\"SecondVal\",\"ThirdVal\";";
            var line2 = $"BA_DEF_DEF_ \"{propertyName}\" \"3\";";

            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.PropertyValueOutOfBound(propertyName, "3"));
            ParseLine(line1, line2, observerMock.Object);
        }

        [Test]
        public void DefaultCustomPropertyEnumWithIntegerValuesIsParsed()
        {
            var dbcString = @"
BO_ 200 SENSOR: 39 SENSOR
 BA_DEF_ BO_ ""AttributeName"" ENUM ""1"",""2"",""3"";
 BA_DEF_DEF_ ""AttributeName"" ""3"";";

            var dbc = Parser.Parse(dbcString);

            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Messages.First().CustomProperties.Values.First().EnumCustomProperty.Value, Is.EqualTo("3"));
        }

        [Test]
        public void DefaultCustomPropertyEnumWithIntegerValuesByIndexIsParsed()
        {
            var dbcString = @"
BO_ 200 SENSOR: 39 SENSOR
 BA_DEF_ BO_ ""AttributeName"" ENUM ""1"",""2"",""3"";
 BA_DEF_DEF_ ""AttributeName"" 2;";

            var dbc = Parser.Parse(dbcString);

            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Messages.First().CustomProperties.Values.First().EnumCustomProperty.Value, Is.EqualTo("3"));
        }
    }
}