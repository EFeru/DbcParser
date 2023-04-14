using System;
using NUnit.Framework;
using System.Linq;
using System.IO;
using DbcParserLib.Model;

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
        public void ParseMessageWithStartBitGreaterThan255Test()
        {
            var dbcString = @"
BO_ 200 SENSOR: 39 SENSOR
 SG_ SENSOR__rear m1 : 256|6@1+ (0.1,0) [0|0] """"  DBG
 SG_ SENSOR__front m1 : 1755|1@1+ (0.1,0) [0|0] """"  DBG";


            var dbc = Parser.Parse(dbcString);

            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(2, dbc.Messages.SelectMany(m => m.Signals).Count());
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
            using (var reader = new StringReader(signal.ValueTable))
            {
                while (reader.Peek() > -1)
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

            var messageIds = new[] { 961, 1041, 1043 };
            var signalCount = new[] { 7, 4, 3 };

            for (var i = 0; i < messageIds.Length; ++i)
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
        public void ManagingOtherKindOfCommentsMultilineTest()
        {
            // This example is taken from kia_ev6.dbc
            var dbcString = @"
BU_: XXX

BO_ 1043 BLINKERS: 8 XXX
 SG_ COUNTER_ALT : 15|4@0+ (1,0) [0|15] """" XXX
 SG_ LEFT_LAMP : 20|1@0+ (1,0) [0|1] """" XXX
 SG_ RIGHT_LAMP : 22|1@0+ (1,0) [0|1] """" XXX
 
CM_ BO_ 1043 ""Message comment first line
second line
third line"";
CM_ BU_ XXX ""Node comment"";
CM_ SG_ 1043 COUNTER_ALT ""only increments on change""; ";

            var dbc = Parser.Parse(dbcString);

            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(1, dbc.Nodes.Count());

            Assert.AreEqual($"Message comment first line{Environment.NewLine}second line{Environment.NewLine}third line", dbc.Messages.First().Comment);
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
VAL_TABLE_ DI_aebLockState 3 ""AEB_LOCK_STATE_SNA"" 2 ""AEB_LOCK_STATE_UNUSED"" 1 ""AEB_LOCK_STATE_UNLOCKED"" 0 ""AEB_LOCK_STATE_LOCKED"" ;

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
 
VAL_ 1043 withNamedTable 3 ""AEB_LOCK_STATE_SNA"" 2 ""AEB_LOCK_STATE_UNUSED"" 1 ""AEB_LOCK_STATE_UNLOCKED"" 0 ""AEB_LOCK_STATE_LOCKED"" ;";

            var dbc = Parser.Parse(dbcString);

            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(0, dbc.Nodes.Count());

            var signal = dbc.Messages.Single().Signals.Single();
            Assert.IsNotNull(signal);
            Assert.AreEqual(107, signal.ValueTable.Length);
        }

        [Test]
        public void UserDefinedAttributesTest()
        {
            var dbcString = @"
BU_: XXX

BO_ 1043 BLINKERS: 8 XXX
 SG_ COUNTER_ALT : 15|4@0+ (1,0) [0|15] """" XXX
 SG_ LEFT_LAMP : 20|1@0+ (1,0) [0|1] """" XXX
 SG_ RIGHT_LAMP : 22|1@0+ (1,0) [0|1] """" XXX
 
BA_DEF_ BU_ ""HexAttribute"" HEX 0 100;
BA_DEF_ BO_ ""IntegerAttribute"" INT 0 10;
BA_DEF_ BO_ ""FloatAttribute"" FLOAT 0 1;
BA_DEF_ SG_ ""StringAttribute"" STRING;
BA_DEF_ SG_ ""EnumAttributeName"" ENUM ""FirstVal"",""SecondVal"",""ThirdVal"";

BA_DEF_DEF_ ""HexAttribute"" 50;
BA_DEF_DEF_ ""IntegerAttribute"" 5;
BA_DEF_DEF_ ""FloatAttribute"" 0.5;
BA_DEF_DEF_ ""StringAttribute"" ""DefaultString"";
BA_DEF_DEF_ ""EnumAttributeName"" ""FirstVal"";

BA_ ""HexAttribute"" BU_ XXX 70;
BA_ ""IntegerAttribute"" BO_ 1043 7;
BA_ ""EnumAttributeName"" SG_ 1043 COUNTER_ALT ""ThirdVal""; ";

            var dbc = Parser.Parse(dbcString);
            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(1, dbc.Nodes.Count());

            var node = dbc.Nodes.First();
            Assert.AreEqual(1, node.CustomProperties.Count());
            Assert.AreEqual(70, node.CustomProperties["HexAttribute"].HexCustomProperty.Value);

            var message = dbc.Messages.First();
            Assert.AreEqual(2, message.CustomProperties.Count());
            Assert.AreEqual(7, message.CustomProperties["IntegerAttribute"].IntegerCustomProperty.Value);
            Assert.AreEqual(0.5, message.CustomProperties["FloatAttribute"].FloatCustomProperty.Value);

            var signal = dbc.Messages.Single().Signals.FirstOrDefault(x => x.Name.Equals("COUNTER_ALT"));
            Assert.IsNotNull(signal);
            Assert.AreEqual(2, signal.CustomProperties.Count());
            Assert.AreEqual("ThirdVal", signal.CustomProperties["EnumAttributeName"].EnumCustomProperty.Value);
            Assert.AreEqual("DefaultString", signal.CustomProperties["StringAttribute"].StringCustomProperty.Value);
        }

        [Test]
        public void EnvironmentVariableTest()
        {
            var dbcString = @"
BU_: XXX

BO_ 1043 BLINKERS: 8 XXX
 SG_ COUNTER_ALT : 15|4@0+ (1,0) [0|15] """" XXX
 SG_ LEFT_LAMP : 20|1@0+ (1,0) [0|1] """" XXX
 SG_ RIGHT_LAMP : 22|1@0+ (1,0) [0|1] """" XXX
 
EV_ EnvVarName: 0 [0|1] """" 0 2 DUMMY_NODE_VECTOR0 XXX; ";

            var dbc = Parser.Parse(dbcString);

            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(1, dbc.Nodes.Count());
            Assert.AreEqual(1, dbc.EnvironmentVariables.Count());

            Assert.AreEqual("EnvVarName", dbc.Nodes.First().EnvironmentVariables.First().Key);
        }

        [Test]
        public void EnvironmentVariableDataTypeIsCorrectlyAppliedTest()
        {
            var dbcString = @"
BU_: XXX

BO_ 1043 BLINKERS: 8 XXX
 SG_ COUNTER_ALT : 15|4@0+ (1,0) [0|15] """" XXX
 SG_ LEFT_LAMP : 20|1@0+ (1,0) [0|1] """" XXX
 SG_ RIGHT_LAMP : 22|1@0+ (1,0) [0|1] """" XXX
 
EV_ EnvVarName: 0 [0|1] """" 0 2 DUMMY_NODE_VECTOR0 XXX;
ENVVAR_DATA_ EnvVarName: 5;";

            var dbc = Parser.Parse(dbcString);

            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(1, dbc.Nodes.Count());
            Assert.AreEqual(1, dbc.EnvironmentVariables.Count());

            Assert.AreEqual("EnvVarName", dbc.Nodes.First().EnvironmentVariables.First().Key);
            Assert.AreEqual(EnvDataType.Data, dbc.Nodes.First().EnvironmentVariables.First().Value.Type);
            Assert.AreEqual(5, dbc.Nodes.First().EnvironmentVariables.First().Value.DataEnvironmentVariable.Length);
        }

        [Test]
        public void EnvironmentVariableWithMultipleNodeTest()
        {
            var dbcString = @"
BU_: XXX YYY

BO_ 1043 BLINKERS: 8 XXX
 SG_ COUNTER_ALT : 15|4@0+ (1,0) [0|15] """" XXX
 SG_ LEFT_LAMP : 20|1@0+ (1,0) [0|1] """" XXX
 SG_ RIGHT_LAMP : 22|1@0+ (1,0) [0|1] """" XXX
 
EV_ EnvVarName: 0 [0|1] """" 0 2 DUMMY_NODE_VECTOR0 XXX,YYY;";

            var dbc = Parser.Parse(dbcString);

            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(2, dbc.Nodes.Count());
            Assert.AreEqual(1, dbc.EnvironmentVariables.Count());

            Assert.AreEqual("EnvVarName", dbc.Nodes.First().EnvironmentVariables.First().Key);
            Assert.AreEqual("EnvVarName", dbc.Nodes.Last().EnvironmentVariables.First().Key);
        }

        [Test]
        public void MultipleEnvironmentVariableToOneNodeTest()
        {
            var dbcString = @"
BU_: XXX

BO_ 1043 BLINKERS: 8 XXX
 SG_ COUNTER_ALT : 15|4@0+ (1,0) [0|15] """" XXX
 SG_ LEFT_LAMP : 20|1@0+ (1,0) [0|1] """" XXX
 SG_ RIGHT_LAMP : 22|1@0+ (1,0) [0|1] """" XXX
 
EV_ EnvVarName1: 0 [0|1] """" 0 2 DUMMY_NODE_VECTOR0 XXX;
EV_ EnvVarName2: 1 [0|1.0] """" 0.5 2 DUMMY_NODE_VECTOR1 XXX;
EV_ EnvVarName3: 0 [0|1] """" 0 2 DUMMY_NODE_VECTOR8000 XXX;
ENVVAR_DATA_ EnvVarName3: 5;";

            var dbc = Parser.Parse(dbcString);

            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(1, dbc.Nodes.Count());
            Assert.AreEqual(3, dbc.EnvironmentVariables.Count());

            Assert.AreEqual("EnvVarName1", dbc.Nodes.First().EnvironmentVariables.First().Key);
            Assert.AreEqual(EnvDataType.Integer, dbc.Nodes.First().EnvironmentVariables.First().Value.Type);

            Assert.AreEqual("EnvVarName2", dbc.Nodes.First().EnvironmentVariables.ElementAt(1).Key);
            Assert.AreEqual(EnvDataType.Float, dbc.Nodes.First().EnvironmentVariables.ElementAt(1).Value.Type);

            Assert.AreEqual("EnvVarName3", dbc.Nodes.First().EnvironmentVariables.Last().Key);
            Assert.AreEqual(EnvDataType.Data, dbc.Nodes.First().EnvironmentVariables.Last().Value.Type);
        }
    }
}