using NUnit.Framework;
using DbcParserLib.Parsers;
using DbcParserLib.Model;
using Moq;
using System.Linq;
using DbcParserLib.Observers;

namespace DbcParserLib.Tests
{
    public class SignalLineParserTests
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

        private static ILineParser CreateParser()
        {
            return new SignalLineParser(new SilentFailureObserver());
        }

        [Test]
        public void EmptyLineIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(signalLineParser.TryParse(string.Empty, dbcBuilderMock.Object, nextLineProviderMock.Object), Is.False);
        }

        [Test]
        public void RandomStartIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(signalLineParser.TryParse("CF_", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.False);
        }

        [Test]
        public void OnlyPrefixWithSpacesIsAcceptedWithNoInteractions()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(signalLineParser.TryParse("SG_        ", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullLineIsParsed()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();

            dbcBuilderMock.Setup(mock => mock.AddSignal(It.IsAny<Signal>()))
                .Callback<Signal>(signal =>
                {
                    Assert.That(signal.Name, Is.EqualTo("MCU_longitude"));
                    Assert.That(signal.StartBit, Is.EqualTo(28));
                    Assert.That(signal.Length, Is.EqualTo(29));
                    Assert.That(signal.ByteOrder, Is.EqualTo(1));
                    Assert.That(signal.ValueType, Is.EqualTo(DbcValueType.Signed));
                    Assert.That(signal.Factor, Is.EqualTo(1E-006));
                    Assert.That(signal.Offset, Is.EqualTo(0));
                    Assert.That(signal.Minimum, Is.EqualTo(-10));
                    Assert.That(signal.Maximum, Is.EqualTo(35.6));
                    Assert.That(string.IsNullOrWhiteSpace(signal.Multiplexing), Is.True);
                    Assert.That(signal.Unit, Is.EqualTo("deg"));
                    Assert.That(signal.Receiver.FirstOrDefault(), Is.EqualTo("NEO"));
                });

            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(signalLineParser.TryParse(@" SG_ MCU_longitude : 28|29@1- (1E-006,0) [-10|35.6] ""deg""  NEO", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullLineMultiplexedIsParsed()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();

            dbcBuilderMock.Setup(mock => mock.AddSignal(It.IsAny<Signal>()))
                .Callback<Signal>(signal =>
                {
                    Assert.That(signal.Name, Is.EqualTo("MCU_longitude"));
                    Assert.That(signal.StartBit, Is.EqualTo(28));
                    Assert.That(signal.Length, Is.EqualTo(29));
                    Assert.That(signal.ByteOrder, Is.EqualTo(1));
                    Assert.That(signal.ValueType, Is.EqualTo(DbcValueType.Signed));
                    Assert.That(signal.Factor, Is.EqualTo(1E-006));
                    Assert.That(signal.Offset, Is.EqualTo(0));
                    Assert.That(signal.Minimum, Is.EqualTo(-10));
                    Assert.That(signal.Maximum, Is.EqualTo(35.6));
                    Assert.That(signal.Multiplexing, Is.EqualTo("m7"));
                    Assert.That(signal.Unit, Is.EqualTo("deg"));
                    Assert.That(signal.Receiver.FirstOrDefault(), Is.EqualTo("NEO"));
                });

            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(signalLineParser.TryParse(@" SG_ MCU_longitude m7 : 28|29@1- (1E-006,0) [-10|35.6] ""deg""  NEO", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullLineMultipleReceiversIsParsed()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();

            dbcBuilderMock.Setup(mock => mock.AddSignal(It.IsAny<Signal>()))
                .Callback<Signal>(signal =>
                {
                    Assert.That(signal.Name, Is.EqualTo("MCU_longitude"));
                    Assert.That(signal.StartBit, Is.EqualTo(28));
                    Assert.That(signal.Length, Is.EqualTo(29));
                    Assert.That(signal.ByteOrder, Is.EqualTo(1));
                    Assert.That(signal.ValueType, Is.EqualTo(DbcValueType.Signed));
                    Assert.That(signal.Factor, Is.EqualTo(1E-006));
                    Assert.That(signal.Offset, Is.EqualTo(0));
                    Assert.That(signal.Minimum, Is.EqualTo(-10));
                    Assert.That(signal.Maximum, Is.EqualTo(35.6));
                    Assert.That(signal.Multiplexing, Is.EqualTo("m7"));
                    Assert.That(signal.Unit, Is.EqualTo("deg"));
                    Assert.That(signal.Receiver, Is.EqualTo(new[] { "NEO", "WHEEL", "TOP" }).AsCollection);

                });

            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(signalLineParser.TryParse(@" SG_ MCU_longitude m7 : 28|29@1- (1E-006,0) [-10|35.6] ""deg""  NEO,WHEEL,TOP", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullLineMultipleReceiversWithSpacesIsParsed()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();

            dbcBuilderMock.Setup(mock => mock.AddSignal(It.IsAny<Signal>()))
                .Callback<Signal>(signal =>
                {
                    Assert.That(signal.Name, Is.EqualTo("MCU_longitude"));
                    Assert.That(signal.StartBit, Is.EqualTo(28));
                    Assert.That(signal.Length, Is.EqualTo(29));
                    Assert.That(signal.ByteOrder, Is.EqualTo(1));
                    Assert.That(signal.ValueType, Is.EqualTo(DbcValueType.Signed));
                    Assert.That(signal.Factor, Is.EqualTo(1E-006));
                    Assert.That(signal.Offset, Is.EqualTo(0));
                    Assert.That(signal.Minimum, Is.EqualTo(-10));
                    Assert.That(signal.Maximum, Is.EqualTo(35.6));
                    Assert.That(signal.Multiplexing, Is.EqualTo("m7"));
                    Assert.That(signal.Unit, Is.EqualTo("deg"));
                    Assert.That(signal.Receiver, Is.EqualTo(new[] { "NEO", "WHEEL", "TOP" }).AsCollection);

                });

            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(signalLineParser.TryParse(@" SG_ MCU_longitude m7 : 28|29@1- (1E-006,0) [-10|35.6] ""deg""  NEO, WHEEL, TOP", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void ParseSignalWithDifferentColonSpaces()
        {
            var dbcString = @"
BO_ 200 SENSOR: 39 SENSOR
 SG_ SENSOR__rear m1: 256|6@1+ (0.1,0) [0|0] """"  DBG
 SG_ SENSOR__front m1 :1755|1@1+ (0.1,0) [0|0] """"  DBG
 SG_ MCU_longitude m7:28|29@1- (1E-006,0) [-10|35.6] ""deg""  NEO";


            var dbc = Parser.Parse(dbcString);

            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Messages.SelectMany(m => m.Signals).Count(), Is.EqualTo(3));
        }

        [TestCase("SG_ qGearboxOilMin : 0|16@1+ (0.1,0) [0|6553.5] \"l/min\" \"NATEC\"")]
        [TestCase("SG_ qGearboxOilMin : 0|16@1+ (0.1,0) [0|6553.5] l/min NATEC")]
        [TestCase("SG_ \"qGearboxOilMin\" : 0|16@1+ (0.1,0) [0|6553.5] \"l/min\" NATEC")]
        [TestCase("SG_ qGearboxOilMin 0|16@1+ (0.1,0) [0|6553.5] \"l/min\" NATEC")]
        [TestCase("SG_ ")]
        public void SignalSyntaxErrorIsObserved(string line)
        {
            var observerMock = m_repository.Create<IParseFailureObserver>();
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            observerMock.Setup(o => o.SignalSyntaxError());

            var lineParser = new SignalLineParser(observerMock.Object);
            lineParser.TryParse(line, dbcBuilderMock.Object, nextLineProviderMock.Object);
        }

        [Test]
        public void SignalMessageNotFoundErrorIsObserved()
        {
            var line = "SG_ qGearboxOilMin : 0|16@1+ (0.1,0) [0|6553.5] \"l/min\" NATEC";

            var observerMock = m_repository.Create<IParseFailureObserver>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();
            var dbcBuilder = new DbcBuilder(observerMock.Object);

            observerMock.Setup(o => o.NoMessageFound());

            var lineParser = new SignalLineParser(observerMock.Object);
            lineParser.TryParse(line, dbcBuilder, nextLineProviderMock.Object);
        }

        [Test]
        public void SignalDuplicateErrorIsObserved()
        {
            var nodeName = "nodeName";
            var signalName = "signalName";
            uint messageId = 123;
            var line1 = $"BU_: {nodeName}";
            var line2 = $"BO_ {messageId} messageName: 8 {nodeName}";
            var line3 = $"SG_ {signalName} : 0|16@1+ (0.1,0) [0|6553.5] \"l/min\" {nodeName}";

            var observerMock = m_repository.Create<IParseFailureObserver>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();
            var dbcBuilder = new DbcBuilder(observerMock.Object);

            observerMock.Setup(o => o.DuplicatedSignalInMessage(messageId, signalName));

            var nodeLineParser = new NodeLineParser(observerMock.Object);
            nodeLineParser.TryParse(line1, dbcBuilder, nextLineProviderMock.Object);

            var messageLineParser = new MessageLineParser(observerMock.Object);
            messageLineParser.TryParse(line2, dbcBuilder, nextLineProviderMock.Object);

            var signalLineParser = new SignalLineParser(observerMock.Object);
            signalLineParser.TryParse(line3, dbcBuilder, nextLineProviderMock.Object);
            signalLineParser.TryParse(line3, dbcBuilder, nextLineProviderMock.Object);
        }
    }
}