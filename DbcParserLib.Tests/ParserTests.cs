using NUnit.Framework;
using System.Linq;
using System.IO;

namespace DbcParserLib.Tests
{
    public class ParserTests
    {
        private const string MainDbcFilePath = @"..\..\..\..\DbcFiles\tesla_can.dbc";

        [Test]
        public void SimpleParseFileTest()
        {
            var dbc = Parser.ParseFromPath(MainDbcFilePath);

            Assert.AreEqual(38, dbc.Messages.Count());
            Assert.AreEqual(485, dbc.Messages.SelectMany(m => m.Signals).Count());
            Assert.AreEqual(15, dbc.Nodes.Count());
        }

        [Test]
        public void ParsingTwiceClearsCollectionsTest()
        {
            // With the new code, this test is quite useless
            var dbc = Parser.ParseFromPath(MainDbcFilePath);
            dbc = Parser.ParseFromPath(MainDbcFilePath);
            Assert.AreEqual(38, dbc.Messages.Count());
            Assert.AreEqual(485, dbc.Messages.SelectMany(m => m.Signals).Count());
            Assert.AreEqual(15, dbc.Nodes.Count());
        }

        [Test]
        public void CheckMessagePropertiesTest()
        {
            var dbc = Parser.ParseFromPath(MainDbcFilePath);

            var targetMessage = dbc.Messages.FirstOrDefault(x => x.ID == 309);
            Assert.IsNotNull(targetMessage);

            Assert.AreEqual("ESP_135h", targetMessage.Name);
            Assert.AreEqual("ESP", targetMessage.Transmitter);
            Assert.AreEqual(5, targetMessage.DLC);
            Assert.AreEqual(19, targetMessage.Signals.Count);
        }

        [Test]
        public void CheckSignalPropertiesTest()
        {
            var dbc = Parser.ParseFromPath(MainDbcFilePath);

            var targetMessage = dbc.Messages.FirstOrDefault(x => x.ID == 1006);
            Assert.IsNotNull(targetMessage);

            Assert.AreEqual(24, targetMessage.Signals.Count);

            var signal = targetMessage.Signals.FirstOrDefault(x => x.Name.Equals("UI_camBlockBlurThreshold"));
            Assert.IsNotNull(signal);
            Assert.AreEqual(0, signal.IsSigned);
            Assert.AreEqual(11, signal.StartBit);
            Assert.AreEqual(6, signal.Length);
            Assert.AreEqual(0.01587, signal.Factor);
            Assert.AreEqual(1, signal.ByteOrder);
            Assert.AreEqual(0, signal.Minimum);
            Assert.AreEqual(1, signal.Maximum);
            Assert.AreEqual(2, signal.Receiver.Length);
        }

        [Test]
        public void CheckOtherSignalPropertiesTest()
        {
            var dbc = Parser.ParseFromPath(MainDbcFilePath);

            var targetMessage = dbc.Messages.FirstOrDefault(x => x.ID == 264);
            Assert.IsNotNull(targetMessage);

            Assert.AreEqual(7, targetMessage.Signals.Count);

            var signal = targetMessage.Signals.FirstOrDefault(x => x.Name.Equals("DI_torqueMotor"));
            Assert.IsNotNull(signal);
            Assert.AreEqual(1, signal.IsSigned);
            Assert.AreEqual("Nm", signal.Unit);
            Assert.AreEqual(13, signal.Length);
            Assert.AreEqual(0.25, signal.Factor);
            Assert.AreEqual(1, signal.ByteOrder);
            Assert.AreEqual(-750, signal.Minimum);
            Assert.AreEqual(750, signal.Maximum);
            Assert.AreEqual(1, signal.Receiver.Length);
        }

        [Test]
        public void CheckTableValuesSignalPropertiesTest()
        {
            var dbc = Parser.ParseFromPath(MainDbcFilePath);

            var targetMessage = dbc.Messages.FirstOrDefault(x => x.ID == 264);
            Assert.IsNotNull(targetMessage);

            Assert.AreEqual(7, targetMessage.Signals.Count);

            var signal = targetMessage.Signals.FirstOrDefault(x => x.Name.Equals("DI_soptState"));
            Assert.IsNotNull(signal);
            Assert.IsFalse(string.IsNullOrWhiteSpace(signal.ValueTable));
            Assert.AreEqual(132, signal.ValueTable.Length);

            var lineCount = 0;
            using(var reader = new StringReader(signal.ValueTable))
            {
                while(reader.Peek() > -1)
                {
                    reader.ReadLine();
                    ++lineCount;
                }
            }

            Assert.AreEqual(6, lineCount);
        }

        [Test]
        public void SignalCommentIsProperlyAppliedWhenMultipleSignalsShareSameNameTest()
        {
            // This example is taken from kia_ev6.dbc
            var dbcString = @"
BO_ 961 BLINKER_STALKS: 8 XXX
 SG_ COUNTER_ALT : 15|4@0+ (1,0) [0|15] """" XXX
 SG_ CHECKSUM_MAYBE : 7|8@0+ (1,0) [0|255] """" XXX
 SG_ HIGHBEAM_FORWARD : 18|1@0+ (1,0) [0|1] """" XXX
 SG_ HIGHBEAM_BACKWARD : 26|1@0+ (1,0) [0|1] """" XXX
 SG_ RIGHT_BLINKER : 32|1@0+ (1,0) [0|1] """" XXX
 SG_ LEFT_BLINKER : 30|1@0+ (1,0) [0|1] """" XXX
 SG_ LIGHT_KNOB_POSITION : 21|2@0+ (1,0) [0|3] """" XXX
 
BO_ 1041 DOORS_SEATBELTS: 8 XXX
 SG_ CHECKSUM_MAYBE : 7|8@0+ (1,0) [0|65535] """" XXX
 SG_ COUNTER_ALT : 15|4@0+ (1,0) [0|15] """" XXX
 SG_ DRIVER_SEATBELT_LATCHED : 42|1@0+ (1,0) [0|1] """" XXX
 SG_ DRIVER_DOOR_OPEN : 24|1@1+ (1,0) [0|1] """" XXX
 
BO_ 1043 BLINKERS: 8 XXX
 SG_ COUNTER_ALT : 15|4@0+ (1,0) [0|15] """" XXX
 SG_ LEFT_LAMP : 20|1@0+ (1,0) [0|1] """" XXX
 SG_ RIGHT_LAMP : 22|1@0+ (1,0) [0|1] """" XXX
 
CM_ SG_ 961 COUNTER_ALT ""only increments on change"";
CM_ SG_ 1041 COUNTER_ALT ""only increments on change"";
CM_ SG_ 1043 COUNTER_ALT ""only increments on change""; ";

            var dbc = Parser.Parse(dbcString);

            Assert.AreEqual(3, dbc.Messages.Count());
            Assert.AreEqual(0, dbc.Nodes.Count());

            var messageIds = new[] { 961, 1041, 1043};
            var signalCount = new[] { 7, 4, 3};

            for(var i = 0; i < messageIds.Length; ++i)
            {
                var targetMessage = dbc.Messages.FirstOrDefault(x => x.ID == messageIds[i]);
                Assert.IsNotNull(targetMessage);

                Assert.AreEqual(signalCount[i], targetMessage.Signals.Count);

                var signal = targetMessage.Signals.FirstOrDefault(x => x.Name.Equals("COUNTER_ALT"));
                Assert.IsNotNull(signal);
                Assert.AreEqual("only increments on change", signal.Comment);
            }
        }

        [Test]
        public void ManagingOtherKindOfCommentsTest()
        {
            // This example is taken from kia_ev6.dbc
            var dbcString = @"
BU_: XXX

BO_ 1043 BLINKERS: 8 XXX
 SG_ COUNTER_ALT : 15|4@0+ (1,0) [0|15] """" XXX
 SG_ LEFT_LAMP : 20|1@0+ (1,0) [0|1] """" XXX
 SG_ RIGHT_LAMP : 22|1@0+ (1,0) [0|1] """" XXX
 
CM_ BO_ 1043 ""Message comment"";
CM_ BU_ XXX ""Node comment"";
CM_ SG_ 1043 COUNTER_ALT ""only increments on change""; ";

            var dbc = Parser.Parse(dbcString);

            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(1, dbc.Nodes.Count());

            Assert.AreEqual("Message comment", dbc.Messages.First().Comment);
            Assert.AreEqual("Node comment", dbc.Nodes.First().Comment);

            var signal = dbc.Messages.Single().Signals.FirstOrDefault(x => x.Name.Equals("COUNTER_ALT"));
            Assert.IsNotNull(signal);
            Assert.AreEqual("only increments on change", signal.Comment);
        }

        [Test]
        public void NamedValTableIsAppliedTest()
        {
            // This example is taken from kia_ev6.dbc
            var dbcString = @"
VAL_TABLE_ DI_aebLockState 3 ""AEB_LOCK_STATE_SNA"" 2 ""AEB_LOCK_STATE_UNUSED"" 1 ""AEB_LOCK_STATE_UNLOCKED"" 0 ""AEB_LOCK_STATE_LOCKED""       ;

BO_ 1043 BLINKERS: 8 XXX
 SG_ withNamedTable : 22|1@0+ (1,0) [0|1] """" XXX
 
VAL_ 1043 withNamedTable DI_aebLockState ; ";

            var dbc = Parser.Parse(dbcString);

            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(0, dbc.Nodes.Count());

            var signal = dbc.Messages.Single().Signals.Single();
            Assert.IsNotNull(signal);
            Assert.AreEqual(107, signal.ValueTable.Length);
        }

        [Test]
        public void ExplicitValTableIsAppliedTest()
        {
            // This example is taken from kia_ev6.dbc
            var dbcString = @"
BO_ 1043 BLINKERS: 8 XXX
 SG_ withNamedTable : 22|1@0+ (1,0) [0|1] """" XXX
 
VAL_ 1043 withNamedTable 3 ""AEB_LOCK_STATE_SNA"" 2 ""AEB_LOCK_STATE_UNUSED"" 1 ""AEB_LOCK_STATE_UNLOCKED"" 0 ""AEB_LOCK_STATE_LOCKED""       ;";

            var dbc = Parser.Parse(dbcString);

            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(0, dbc.Nodes.Count());

            var signal = dbc.Messages.Single().Signals.Single();
            Assert.IsNotNull(signal);
            Assert.AreEqual(107, signal.ValueTable.Length);
        }
    }
}