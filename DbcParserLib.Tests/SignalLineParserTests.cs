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

            Assert.IsFalse(signalLineParser.TryParse(string.Empty, dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void RandomStartIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(signalLineParser.TryParse("CF_", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void OnlyPrefixWithSpacesIsAcceptedWithNoInteractions()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(signalLineParser.TryParse("SG_        ", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void FullLineIsParsed()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();

            dbcBuilderMock.Setup(mock => mock.AddSignal(It.IsAny<Signal>()))
                .Callback<Signal>(signal =>
                {
                    Assert.AreEqual("MCU_longitude", signal.Name);
                    Assert.AreEqual(28, signal.StartBit);
                    Assert.AreEqual(29, signal.Length);
                    Assert.AreEqual(1, signal.ByteOrder);
                    Assert.AreEqual(1, signal.IsSigned);
                    Assert.AreEqual(1E-006, signal.Factor);
                    Assert.AreEqual(0, signal.Offset);
                    Assert.AreEqual(-10, signal.Minimum);
                    Assert.AreEqual(35.6, signal.Maximum);
                    Assert.IsTrue(string.IsNullOrWhiteSpace(signal.Multiplexing));
                    Assert.AreEqual("deg", signal.Unit);
                    Assert.AreEqual("NEO", signal.Receiver.FirstOrDefault());
                });

            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(signalLineParser.TryParse(@" SG_ MCU_longitude : 28|29@1- (1E-006,0) [-10|35.6] ""deg""  NEO", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void FullLineMultiplexedIsParsed()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();

            dbcBuilderMock.Setup(mock => mock.AddSignal(It.IsAny<Signal>()))
                .Callback<Signal>(signal =>
                {
                    Assert.AreEqual("MCU_longitude", signal.Name);
                    Assert.AreEqual(28, signal.StartBit);
                    Assert.AreEqual(29, signal.Length);
                    Assert.AreEqual(1, signal.ByteOrder);
                    Assert.AreEqual(1, signal.IsSigned);
                    Assert.AreEqual(1E-006, signal.Factor);
                    Assert.AreEqual(0, signal.Offset);
                    Assert.AreEqual(-10, signal.Minimum);
                    Assert.AreEqual(35.6, signal.Maximum);
                    Assert.AreEqual("m7", signal.Multiplexing);
                    Assert.AreEqual("deg", signal.Unit);
                    Assert.AreEqual("NEO", signal.Receiver.FirstOrDefault());
                });

            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(signalLineParser.TryParse(@" SG_ MCU_longitude m7 : 28|29@1- (1E-006,0) [-10|35.6] ""deg""  NEO", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void FullLineMultipleReceiversIsParsed()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();

            dbcBuilderMock.Setup(mock => mock.AddSignal(It.IsAny<Signal>()))
                .Callback<Signal>(signal =>
                {
                    Assert.AreEqual("MCU_longitude", signal.Name);
                    Assert.AreEqual(28, signal.StartBit);
                    Assert.AreEqual(29, signal.Length);
                    Assert.AreEqual(1, signal.ByteOrder);
                    Assert.AreEqual(1, signal.IsSigned);
                    Assert.AreEqual(1E-006, signal.Factor);
                    Assert.AreEqual(0, signal.Offset);
                    Assert.AreEqual(-10, signal.Minimum);
                    Assert.AreEqual(35.6, signal.Maximum);
                    Assert.AreEqual("m7", signal.Multiplexing);
                    Assert.AreEqual("deg", signal.Unit);
                    CollectionAssert.AreEqual(new[] { "NEO", "WHEEL", "TOP" }, signal.Receiver);

                });

            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(signalLineParser.TryParse(@" SG_ MCU_longitude m7 : 28|29@1- (1E-006,0) [-10|35.6] ""deg""  NEO,WHEEL,TOP", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void FullLineMultipleReceiversWithSpacesIsParsed()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();

            dbcBuilderMock.Setup(mock => mock.AddSignal(It.IsAny<Signal>()))
                .Callback<Signal>(signal =>
                {
                    Assert.AreEqual("MCU_longitude", signal.Name);
                    Assert.AreEqual(28, signal.StartBit);
                    Assert.AreEqual(29, signal.Length);
                    Assert.AreEqual(1, signal.ByteOrder);
                    Assert.AreEqual(1, signal.IsSigned);
                    Assert.AreEqual(1E-006, signal.Factor);
                    Assert.AreEqual(0, signal.Offset);
                    Assert.AreEqual(-10, signal.Minimum);
                    Assert.AreEqual(35.6, signal.Maximum);
                    Assert.AreEqual("m7", signal.Multiplexing);
                    Assert.AreEqual("deg", signal.Unit);
                    CollectionAssert.AreEqual(new[] { "NEO", "WHEEL", "TOP" }, signal.Receiver);

                });

            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(signalLineParser.TryParse(@" SG_ MCU_longitude m7 : 28|29@1- (1E-006,0) [-10|35.6] ""deg""  NEO, WHEEL, TOP", dbcBuilderMock.Object, nextLineProviderMock.Object));
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

            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(3, dbc.Messages.SelectMany(m => m.Signals).Count());
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