using DbcParserLib.Observers;
using NUnit.Framework;
using System.Linq;

namespace DbcParserLib.Tests
{
    [TestFixture]
    public class ExtraMessageTransmitterLineParserTests
    {
        [Test]
        public void ParseOneExtraTransmitters()
        {
            var dbcString = @"
BO_ 200 TestMessage: 1 TestTransmitter
 SG_ Test : 0|8@1+ (0.1,0) [0|0] """"  DBG

BO_TX_BU_ 200 : Transmitter2;";

            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.Multiple(() =>
            {
                Assert.That(errorList, Has.Count.EqualTo(0));
                Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.SelectMany(m => m.Signals).Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().AdditionalTransmitters.Count, Is.EqualTo(1));
                Assert.That(dbc.Messages.First().AdditionalTransmitters.First(), Is.EqualTo("Transmitter2"));
            });
        }

        [Test]
        public void ParseTwoExtraTransmitters()
        {
            var dbcString = @"
BO_ 200 TestMessage: 1 TestTransmitter
 SG_ Test : 0|8@1+ (0.1,0) [0|0] """"  DBG

BO_TX_BU_ 200 : Transmitter2,Transmitter3;";


            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.Multiple(() =>
            {
                Assert.That(errorList, Has.Count.EqualTo(0));
                Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.SelectMany(m => m.Signals).Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().AdditionalTransmitters.Count, Is.EqualTo(2));
                Assert.That(dbc.Messages.First().AdditionalTransmitters.First(), Is.EqualTo("Transmitter2"));
                Assert.That(dbc.Messages.First().AdditionalTransmitters.Last(), Is.EqualTo("Transmitter3"));
            });
        }

        [Test]
        public void MissingLineTerminationParsingErrorIsObserved()
        {
            var dbcString = @"BO_TX_BU_ 200 : Transmitter2";

            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.That(errorList, Has.Count.EqualTo(1));
        }

        [Test]
        public void LineTerminationWithLeadingSpace()
        {
            var dbcString = @"
BO_ 200 TestMessage: 1 TestTransmitter
 SG_ Test : 0|8@1+ (0.1,0) [0|0] """"  DBG

BO_TX_BU_ 200 : Transmitter2 ;";

            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.Multiple(() =>
            {
                Assert.That(errorList, Has.Count.EqualTo(0));
                Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.SelectMany(m => m.Signals).Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().AdditionalTransmitters.Count, Is.EqualTo(1));
                Assert.That(dbc.Messages.First().AdditionalTransmitters.First(), Is.EqualTo("Transmitter2"));
            });
        }

        [Test]
        public void LineTerminationWithLeadingSpaceMultiDefinition()
        {
            var dbcString = @"
BO_ 200 TestMessage: 1 TestTransmitter
 SG_ Test : 0|8@1+ (0.1,0) [0|0] """"  DBG

BO_TX_BU_ 200 : Transmitter2, Transmitter3 ;";

            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.Multiple(() =>
            {
                Assert.That(errorList, Has.Count.EqualTo(0));
                Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.SelectMany(m => m.Signals).Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().AdditionalTransmitters.Count, Is.EqualTo(2));
                Assert.That(dbc.Messages.First().AdditionalTransmitters.First(), Is.EqualTo("Transmitter2"));
                Assert.That(dbc.Messages.First().AdditionalTransmitters.Last(), Is.EqualTo("Transmitter3"));
            });
        }

        [Test]
        public void ParseTwoExtraTransmittersLessSpaces()
        {
            var dbcString = @"
BO_ 200 TestMessage: 1 TestTransmitter
 SG_ Test : 0|8@1+ (0.1,0) [0|0] """"  DBG

BO_TX_BU_ 200:Transmitter2,Transmitter3;";


            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.Multiple(() =>
            {
                Assert.That(errorList, Has.Count.EqualTo(0));
                Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.SelectMany(m => m.Signals).Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().AdditionalTransmitters.Count, Is.EqualTo(2));
                Assert.That(dbc.Messages.First().AdditionalTransmitters.First(), Is.EqualTo("Transmitter2"));
                Assert.That(dbc.Messages.First().AdditionalTransmitters.Last(), Is.EqualTo("Transmitter3"));
            });
        }

        [Test]
        public void ParseTwoExtraTransmittersMoreSpaces()
        {
            var dbcString = @"
BO_ 200 TestMessage: 1 TestTransmitter
 SG_ Test : 0|8@1+ (0.1,0) [0|0] """"  DBG

BO_TX_BU_ 200 : Transmitter2 , Transmitter3 ;";


            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.Multiple(() =>
            {
                Assert.That(errorList, Has.Count.EqualTo(0));
                Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.SelectMany(m => m.Signals).Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().AdditionalTransmitters.Count, Is.EqualTo(2));
                Assert.That(dbc.Messages.First().AdditionalTransmitters.First(), Is.EqualTo("Transmitter2"));
                Assert.That(dbc.Messages.First().AdditionalTransmitters.Last(), Is.EqualTo("Transmitter3"));
            });
        }

        [Test]
        public void ParsingErrorIsObserved()
        {
            var dbcString = @"BO_TX_BU_ 200 ; Transmitter2, Transmitter3;";

            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.That(errorList, Has.Count.EqualTo(1));
        }

        [Test]
        public void ParseExtraTransmittersDuplicateErrorIsObserved()
        {
            var dbcString = @"
BO_ 200 TestMessage: 1 TestTransmitter
 SG_ Test : 0|8@1+ (0.1,0) [0|0] """"  DBG

BO_TX_BU_ 200 : Transmitter2 , Transmitter2 ;";


            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.Multiple(() =>
            {
                Assert.That(errorList, Has.Count.EqualTo(1));
                Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.SelectMany(m => m.Signals).Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().AdditionalTransmitters.Count, Is.EqualTo(1));
                Assert.That(dbc.Messages.First().AdditionalTransmitters.First(), Is.EqualTo("Transmitter2"));
            });
        }

        [Test]
        public void ParseExtraTransmittersMessageNotFoundErrorIsObserved()
        {
            var dbcString = @"
BO_ 200 TestMessage: 1 TestTransmitter
 SG_ Test : 0|8@1+ (0.1,0) [0|0] """"  DBG

BO_TX_BU_ 201 : Transmitter2, Transmitter3 ;";

            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.Multiple(() =>
            {
                Assert.That(errorList, Has.Count.EqualTo(1));
            });
        }
    }
}
