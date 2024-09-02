using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using DbcParserLib.Model;
using DbcParserLib.Observers;

namespace DbcParserLib.Tests
{
    public class ParserTests
    {
        private const string MainDbcFilePath = @"..\..\..\..\DbcFiles\tesla_can.dbc";

        [Test]
        public void SimpleParseFileTest()
        {
            var dbc = Parser.ParseFromPath(MainDbcFilePath);

            Assert.That(dbc.Messages.Count(), Is.EqualTo(38));
            Assert.That(dbc.Messages.SelectMany(m => m.Signals).Count(), Is.EqualTo(485));
            Assert.That(dbc.Nodes.Count(), Is.EqualTo(15));
        }
        [Test]
        public void ParseMessageWithStartBitGreaterThan255Test()
        {
            var dbcString = @"
BO_ 200 SENSOR: 39 SENSOR
 SG_ SENSOR__rear m1 : 256|6@1+ (0.1,0) [0|0] """"  DBG
 SG_ SENSOR__front m1 : 1755|1@1+ (0.1,0) [0|0] """"  DBG";


            var dbc = Parser.Parse(dbcString);

            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Messages.SelectMany(m => m.Signals).Count(), Is.EqualTo(2));
        }

        [Test]
        public void ParsingTwiceClearsCollectionsTest()
        {
            // With the new code, this test is quite useless
            var dbc = Parser.ParseFromPath(MainDbcFilePath);
            dbc = Parser.ParseFromPath(MainDbcFilePath);
            Assert.That(dbc.Messages.Count(), Is.EqualTo(38));
            Assert.That(dbc.Messages.SelectMany(m => m.Signals).Count(), Is.EqualTo(485));
            Assert.That(dbc.Nodes.Count(), Is.EqualTo(15));
        }

        [Test]
        public void CheckMessagePropertiesTest()
        {
            var dbc = Parser.ParseFromPath(MainDbcFilePath);

            var targetMessage = dbc.Messages.FirstOrDefault(x => x.ID == 309);
            Assert.That(targetMessage, Is.Not.Null);

            Assert.That(targetMessage.Name, Is.EqualTo("ESP_135h"));
            Assert.That(targetMessage.Transmitter, Is.EqualTo("ESP"));
            Assert.That(targetMessage.DLC, Is.EqualTo(5));
            Assert.That(targetMessage.Signals.Count, Is.EqualTo(19));
        }

        [Test]
        public void CheckSignalPropertiesTest()
        {
            var dbc = Parser.ParseFromPath(MainDbcFilePath);

            var targetMessage = dbc.Messages.FirstOrDefault(x => x.ID == 1006);
            Assert.That(targetMessage, Is.Not.Null);

            Assert.That(targetMessage.Signals.Count, Is.EqualTo(24));

            var signal = targetMessage.Signals.FirstOrDefault(x => x.Name.Equals("UI_camBlockBlurThreshold"));
            Assert.That(signal, Is.Not.Null);
            Assert.That(signal.ValueType, Is.EqualTo(DbcValueType.Unsigned));
            Assert.That(signal.StartBit, Is.EqualTo(11));
            Assert.That(signal.Length, Is.EqualTo(6));
            Assert.That(signal.Factor, Is.EqualTo(0.01587));
            Assert.That(signal.ByteOrder, Is.EqualTo(1));
            Assert.That(signal.Minimum, Is.EqualTo(0));
            Assert.That(signal.Maximum, Is.EqualTo(1));
            Assert.That(signal.Receiver.Length, Is.EqualTo(2));
        }

        [Test]
        public void CheckOtherSignalPropertiesTest()
        {
            var dbc = Parser.ParseFromPath(MainDbcFilePath);

            var targetMessage = dbc.Messages.FirstOrDefault(x => x.ID == 264);
            Assert.That(targetMessage, Is.Not.Null);

            Assert.That(targetMessage.Signals.Count, Is.EqualTo(7));

            var signal = targetMessage.Signals.FirstOrDefault(x => x.Name.Equals("DI_torqueMotor"));
            Assert.That(signal, Is.Not.Null);
            Assert.That(signal.ValueType, Is.EqualTo(DbcValueType.Signed));
            Assert.That(signal.Unit, Is.EqualTo("Nm"));
            Assert.That(signal.Length, Is.EqualTo(13));
            Assert.That(signal.Factor, Is.EqualTo(0.25));
            Assert.That(signal.ByteOrder, Is.EqualTo(1));
            Assert.That(signal.Minimum, Is.EqualTo(-750));
            Assert.That(signal.Maximum, Is.EqualTo(750));
            Assert.That(signal.Receiver.Length, Is.EqualTo(1));
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

            Assert.That(dbc.Messages.Count(), Is.EqualTo(3));
            Assert.That(dbc.Nodes.Count(), Is.EqualTo(0));

            var messageIds = new[] { 961, 1041, 1043 };
            var signalCount = new[] { 7, 4, 3 };

            for (var i = 0; i < messageIds.Length; ++i)
            {
                var targetMessage = dbc.Messages.FirstOrDefault(x => x.ID == messageIds[i]);
                Assert.That(targetMessage, Is.Not.Null);

                Assert.That(targetMessage.Signals.Count, Is.EqualTo(signalCount[i]));

                var signal = targetMessage.Signals.FirstOrDefault(x => x.Name.Equals("COUNTER_ALT"));
                Assert.That(signal, Is.Not.Null);
                Assert.That(signal.Comment, Is.EqualTo("only increments on change"));
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

            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Nodes.Count(), Is.EqualTo(1));

            Assert.That(dbc.Messages.First().Comment, Is.EqualTo("Message comment"));
            Assert.That(dbc.Nodes.First().Comment, Is.EqualTo("Node comment"));

            var signal = dbc.Messages.Single().Signals.FirstOrDefault(x => x.Name.Equals("COUNTER_ALT"));
            Assert.That(signal, Is.Not.Null);
            Assert.That(signal.Comment, Is.EqualTo("only increments on change"));
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

            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Nodes.Count(), Is.EqualTo(1));

            Assert.That(dbc.Messages.First().Comment, Is.EqualTo($"Message comment first line{Environment.NewLine}second line{Environment.NewLine}third line"));
            Assert.That(dbc.Nodes.First().Comment, Is.EqualTo("Node comment"));

            var signal = dbc.Messages.Single().Signals.FirstOrDefault(x => x.Name.Equals("COUNTER_ALT"));
            Assert.That(signal, Is.Not.Null);
            Assert.That(signal.Comment, Is.EqualTo("only increments on change"));
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

            var expectedValueTableMap = new Dictionary<int, string>()
            {
                { 3, "AEB_LOCK_STATE_SNA" },
                { 2, "AEB_LOCK_STATE_UNUSED" },
                { 1, "AEB_LOCK_STATE_UNLOCKED" },
                { 0, "AEB_LOCK_STATE_LOCKED"}
            };

            var dbc = Parser.Parse(dbcString);

            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Nodes.Count(), Is.EqualTo(0));

            var signal = dbc.Messages.Single().Signals.Single();
            Assert.That(signal, Is.Not.Null);
            Assert.That(signal.ValueTableMap, Is.EqualTo(expectedValueTableMap));
        }

        [Test]
        public void ExplicitValTableIsAppliedTest()
        {
            // This example is taken from kia_ev6.dbc
            var dbcString = @"
BO_ 1043 BLINKERS: 8 XXX
 SG_ withNamedTable : 22|1@0+ (1,0) [0|1] """" XXX
 
VAL_ 1043 withNamedTable 3 ""AEB_LOCK_STATE_SNA"" 2 ""AEB_LOCK_STATE_UNUSED"" 1 ""AEB_LOCK_STATE_UNLOCKED"" 0 ""AEB_LOCK_STATE_LOCKED"" ;";

            var expectedValueTableMap = new Dictionary<int, string>()
            {
                { 3, "AEB_LOCK_STATE_SNA" },
                { 2, "AEB_LOCK_STATE_UNUSED" },
                { 1, "AEB_LOCK_STATE_UNLOCKED" },
                { 0, "AEB_LOCK_STATE_LOCKED"}
            };

            var dbc = Parser.Parse(dbcString);

            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Nodes.Count(), Is.EqualTo(0));

            var signal = dbc.Messages.Single().Signals.Single();
            Assert.That(signal, Is.Not.Null);
            Assert.That(signal.ValueTableMap, Is.EqualTo(expectedValueTableMap));
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
            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Nodes.Count(), Is.EqualTo(1));

            var node = dbc.Nodes.First();
            Assert.That(node.CustomProperties.Count(), Is.EqualTo(1));
            Assert.That(node.CustomProperties["HexAttribute"].HexCustomProperty.Value, Is.EqualTo(70));

            var message = dbc.Messages.First();
            Assert.That(message.CustomProperties.Count(), Is.EqualTo(2));
            Assert.That(message.CustomProperties["IntegerAttribute"].IntegerCustomProperty.Value, Is.EqualTo(7));
            Assert.That(message.CustomProperties["FloatAttribute"].FloatCustomProperty.Value, Is.EqualTo(0.5));

            var signal = dbc.Messages.Single().Signals.FirstOrDefault(x => x.Name.Equals("COUNTER_ALT"));
            Assert.That(signal, Is.Not.Null);
            Assert.That(signal.CustomProperties.Count(), Is.EqualTo(2));
            Assert.That(signal.CustomProperties["EnumAttributeName"].EnumCustomProperty.Value, Is.EqualTo("ThirdVal"));
            Assert.That(signal.CustomProperties["StringAttribute"].StringCustomProperty.Value, Is.EqualTo("DefaultString"));
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

            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Nodes.Count(), Is.EqualTo(1));
            Assert.That(dbc.EnvironmentVariables.Count(), Is.EqualTo(1));

            Assert.That(dbc.Nodes.First().EnvironmentVariables.First().Key, Is.EqualTo("EnvVarName"));
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

            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Nodes.Count(), Is.EqualTo(1));
            Assert.That(dbc.EnvironmentVariables.Count(), Is.EqualTo(1));

            Assert.That(dbc.Nodes.First().EnvironmentVariables.First().Key, Is.EqualTo("EnvVarName"));
            Assert.That(dbc.Nodes.First().EnvironmentVariables.First().Value.Type, Is.EqualTo(EnvDataType.Data));
            Assert.That(dbc.Nodes.First().EnvironmentVariables.First().Value.DataEnvironmentVariable.Length, Is.EqualTo(5));
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

            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Nodes.Count(), Is.EqualTo(2));
            Assert.That(dbc.EnvironmentVariables.Count(), Is.EqualTo(1));

            Assert.That(dbc.Nodes.First().EnvironmentVariables.First().Key, Is.EqualTo("EnvVarName"));
            Assert.That(dbc.Nodes.Last().EnvironmentVariables.First().Key, Is.EqualTo("EnvVarName"));
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

            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Nodes.Count(), Is.EqualTo(1));
            Assert.That(dbc.EnvironmentVariables.Count(), Is.EqualTo(3));

            Assert.That(dbc.Nodes.First().EnvironmentVariables.First().Key, Is.EqualTo("EnvVarName1"));
            Assert.That(dbc.Nodes.First().EnvironmentVariables.First().Value.Type, Is.EqualTo(EnvDataType.Integer));

            Assert.That(dbc.Nodes.First().EnvironmentVariables.ElementAt(1).Key, Is.EqualTo("EnvVarName2"));
            Assert.That(dbc.Nodes.First().EnvironmentVariables.ElementAt(1).Value.Type, Is.EqualTo(EnvDataType.Float));

            Assert.That(dbc.Nodes.First().EnvironmentVariables.Last().Key, Is.EqualTo("EnvVarName3"));
            Assert.That(dbc.Nodes.First().EnvironmentVariables.Last().Value.Type, Is.EqualTo(EnvDataType.Data));
        }

        [Test]
        public void CheckGlobalPropertiesTest()
        {
            var file = @"..\..\..\..\DbcFiles\ext_multiplexed.dbc";
            var dbc = Parser.ParseFromPath(file);

            Assert.That(dbc.GlobalProperties.Count(), Is.EqualTo(3));

            var dbName = dbc.GlobalProperties.FirstOrDefault(x => x.CustomPropertyDefinition.Name.Equals("DBName"));
            Assert.That(dbName, Is.Not.Null);
            Assert.That(dbName.CustomPropertyDefinition.DataType, Is.EqualTo(CustomPropertyDataType.String));
            Assert.That(dbName.CustomPropertyDefinition.StringCustomProperty, Is.Not.Null);
            Assert.That(dbName.CustomPropertyDefinition.StringCustomProperty.Default, Is.EqualTo(string.Empty));
            Assert.That(dbName.StringCustomProperty, Is.Not.Null);
            Assert.That(dbName.StringCustomProperty.Value, Is.EqualTo("z_mx"));

            var busType = dbc.GlobalProperties.FirstOrDefault(x => x.CustomPropertyDefinition.Name.Equals("BusType"));
            Assert.That(busType, Is.Not.Null);
            Assert.That(busType.CustomPropertyDefinition.DataType, Is.EqualTo(CustomPropertyDataType.String));
            Assert.That(busType.CustomPropertyDefinition.StringCustomProperty, Is.Not.Null);
            Assert.That(busType.CustomPropertyDefinition.StringCustomProperty.Default, Is.EqualTo("CAN"));
            Assert.That(busType.StringCustomProperty, Is.Not.Null);
            Assert.That(busType.StringCustomProperty.Value, Is.EqualTo("CAN"));

            var protocolType = dbc.GlobalProperties.FirstOrDefault(x => x.CustomPropertyDefinition.Name.Equals("ProtocolType"));
            Assert.That(protocolType, Is.Not.Null);
            Assert.That(protocolType.CustomPropertyDefinition.DataType, Is.EqualTo(CustomPropertyDataType.String));
            Assert.That(protocolType.CustomPropertyDefinition.StringCustomProperty, Is.Not.Null);
            Assert.That(protocolType.CustomPropertyDefinition.StringCustomProperty.Default, Is.EqualTo("J1939"));
            Assert.That(protocolType.StringCustomProperty, Is.Not.Null);
            Assert.That(protocolType.StringCustomProperty.Value, Is.EqualTo("J1939"));


            var targetMessage = dbc.Messages.FirstOrDefault(x => x.ID == 201391870); // Extended ID
            Assert.That(targetMessage, Is.Not.Null);
            Assert.That(targetMessage.CustomProperties, Has.Count.EqualTo(10));
            Assert.That(targetMessage.CustomProperties["VFrameFormat"].EnumCustomProperty.Value, Is.EqualTo("J1939PG"));

            Assert.That(targetMessage.Signals.Count, Is.EqualTo(8));

            var floatSignal = targetMessage.Signals.FirstOrDefault(x => x.Name.Equals("S6"));
            Assert.That(floatSignal, Is.Not.Null);
            Assert.That(floatSignal.ValueType, Is.EqualTo(DbcValueType.IEEEFloat)); // Set with a property

            // Should check the extended multiplexing stuff once implemented
        }

        [Test]
        public void MultilineValueTableTest()
        {
            var dbcString = @"
BO_ 134 TestMessage: 1 Test
 SG_ TEST_BuckleSwitch : 0|8@0+ (1,0) [0|0] """" Receiver

VAL_ 134 TEST_BuckleSwitch 0 ""Buckled "" 1 "" Unbuckle "" 2 ""Not Used"" 3 ""Not Used
Default value: 0x0"";";

            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.That(errorList, Has.Count.EqualTo(0));
            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Messages.First().Signals.Count(), Is.EqualTo(1));
            Assert.That(dbc.Messages.First().Signals.First().ValueTableMap.Count, Is.EqualTo(4));
            Assert.That(dbc.Messages.First().Signals.First().ValueTableMap.Last().Key, Is.EqualTo(3));
            Assert.That(dbc.Messages.First().Signals.First().ValueTableMap.Last().Value, Is.EqualTo("Not Used Default value: 0x0"));
        }

        [Test]
        public void MultilineValueTableTestWithOnlyNewLine()
        {
            var dbcString = "BO_ 134 TestMessage: 1 Test \n SG_ TEST_BuckleSwitch : 0|8@0+ (1,0) [0|0] \"\" Receiver\n VAL_ 134 TEST_BuckleSwitch 0 \"Buckled \" 1 \" Unbuckle \" 2 \"Not Used\" 3 \"Not Used \nDefault value: 0x0\";";

            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.That(errorList, Has.Count.EqualTo(0));
            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Messages.First().Signals.Count(), Is.EqualTo(1));
            Assert.That(dbc.Messages.First().Signals.First().ValueTableMap.Count, Is.EqualTo(4));
            Assert.That(dbc.Messages.First().Signals.First().ValueTableMap.Last().Key, Is.EqualTo(3));
            Assert.That(dbc.Messages.First().Signals.First().ValueTableMap.Last().Value, Is.EqualTo("Not Used Default value: 0x0"));
        }

        [Test]
        public void MultilineSignalTestEOF()
        {
            var dbcString = @"BO_ 1160 DAS_steeringControl: 4 NEO
 SG_ DAS_steeringControlType : 23|2@0+ 
 (1,0) [0|0] """"  EPAS";

            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.That(errorList, Has.Count.EqualTo(0));
            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Messages.First().Signals.Count(), Is.EqualTo(1));
            Assert.That(dbc.Messages.First().Signals.First().Receiver.Count, Is.EqualTo(1));
            Assert.That(dbc.Messages.First().Signals.First().Receiver.First(), Is.EqualTo("EPAS"));
        }

        [Test]
        public void MultilineSignalTestNextLineEmpty()
        {
            var dbcString = @"BO_ 1160 DAS_steeringControl: 4 NEO
 SG_ DAS_steeringControlType : 23|2@0+ 
 (1,0) [0|0] """"  EPAS
";

            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.That(errorList, Has.Count.EqualTo(0));
            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Messages.First().Signals.Count(), Is.EqualTo(1));
            Assert.That(dbc.Messages.First().Signals.First().Receiver.Count, Is.EqualTo(1));
            Assert.That(dbc.Messages.First().Signals.First().Receiver.First(), Is.EqualTo("EPAS"));
        }

        [Test]
        public void MultilineSignalTestNextLineOtherDefinition()
        {
            var dbcString = @"BO_ 1160 DAS_steeringControl: 4 NEO
 SG_ DAS_steeringControlType : 23|2@0+ 
 (1,0) [0|0] """"  EPAS
 SG_ DAS_steeringAngleRequest : 0|16@0+ (1,0) [0|0] """"  EPAS
";

            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.That(errorList, Has.Count.EqualTo(0));
            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Messages.First().Signals.Count(), Is.EqualTo(2));
            Assert.That(dbc.Messages.First().Signals.First().Receiver.Count, Is.EqualTo(1));
            Assert.That(dbc.Messages.First().Signals.First().Receiver.First(), Is.EqualTo("EPAS"));
        }

        [Test]
        public void SingleLineMultiValueTableTest()
        {
            var dbcString = @"BO_ 1160 DAS_steeringControl: 4 NEO
 SG_ DAS_steeringControlType : 23|2@0+ (1,0) [0|0] """"  EPAS
 SG_ DAS_steeringAngleRequest : 0|16@0+ (1,0) [0|0] """"  EPAS

VAL_ 1160 DAS_steeringControlType 1 ""ANGLE_CONTROL"" 3 ""DISABLED"" 0 ""NONE"" 2 ""RESERVED"" ; VAL_ 1160 DAS_steeringAngleRequest 16384 ""ZERO_ANGLE"" ;";

            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.That(errorList, Has.Count.EqualTo(0));
            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Messages.First().Signals.Count(), Is.EqualTo(2));
            Assert.That(dbc.Messages.First().Signals.First().ValueTableMap.Count, Is.EqualTo(4));
            Assert.That(dbc.Messages.First().Signals.First().ValueTableMap.Last().Key, Is.EqualTo(2));
            Assert.That(dbc.Messages.First().Signals.First().ValueTableMap.Last().Value, Is.EqualTo("RESERVED"));
            Assert.That(dbc.Messages.First().Signals.Last().ValueTableMap.Count, Is.EqualTo(1));
            Assert.That(dbc.Messages.First().Signals.Last().ValueTableMap.Last().Key, Is.EqualTo(16384));
            Assert.That(dbc.Messages.First().Signals.Last().ValueTableMap.Last().Value, Is.EqualTo("ZERO_ANGLE"));
        }

        [Test]
        public void CommentWithEmptyLines()
        {
            var dbcString = @"
BO_ 1043 BLINKERS: 8 XXX
 
CM_ BO_ 1043 ""Message comment first line

third line"";";


            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.That(errorList, Has.Count.EqualTo(0));
            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Messages.First().Comment, Is.EqualTo($"Message comment first line{Environment.NewLine}{Environment.NewLine}third line"));
        }

        [Test]
        public void CommentEndLineWithTermination()
        {
            var dbcString = @"
BO_ 1043 BLINKERS: 8 XXX
 
CM_ BO_ 1043 ""Message comment first line;
second line"";";


            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.That(errorList, Has.Count.EqualTo(0));
            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Messages.First().Comment, Is.EqualTo($"Message comment first line;{Environment.NewLine}second line"));
        }

        [Test]
        public void CommentContainingTermination()
        {
            var dbcString = @"
BO_ 1043 BLINKERS: 8 XXX
 
CM_ BO_ 1043 ""Message; comment; first; line
second; line"";";


            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.That(errorList, Has.Count.EqualTo(0));
            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Messages.First().Comment, Is.EqualTo($"Message; comment; first; line{Environment.NewLine}second; line"));
        }

        [Test]
        public void CommentMissingTerminationGetsCorrected()
        {
            var dbcString = @"
BO_ 1043 BLINKERS: 8 XXX
 
CM_ BO_ 1043 ""Message comment first line
second line""";


            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.That(errorList, Has.Count.EqualTo(0));
            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Messages.First().Comment, Is.EqualTo($"Message comment first line{Environment.NewLine}second line"));
        }

        [Test]
        public void HandleMultiDefinitionLineAfterMultiLine()
        {
            var dbcString = @"
BO_ 1160 DAS_steeringControl: 4 NEO
 SG_ DAS_steeringControlType : 23|2@0+ (1,0) [0|0] """"  EPAS
 SG_ DAS_steeringAngleRequest : 0|16@0+ (1,0) [0|0] """"  EPAS
 
CM_ BO_ 1160 ""Message comment first line
second line""; VAL_ 1160 DAS_steeringControlType 1 ""ANGLE_CONTROL"" 3 ""DISABLED"" 0 ""NONE"" 2 ""RESERVED""; VAL_ 1160 DAS_steeringAngleRequest 16384 ""ZERO_ANGLE""; ";


            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.That(errorList, Has.Count.EqualTo(0));
            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Messages.First().Comment, Is.EqualTo($"Message comment first line{Environment.NewLine}second line"));

            Assert.That(dbc.Messages.First().Signals.Count(), Is.EqualTo(2));
            Assert.That(dbc.Messages.First().Signals.First().ValueTableMap.Count, Is.EqualTo(4));
            Assert.That(dbc.Messages.First().Signals.First().ValueTableMap.Last().Key, Is.EqualTo(2));
            Assert.That(dbc.Messages.First().Signals.First().ValueTableMap.Last().Value, Is.EqualTo("RESERVED"));
            Assert.That(dbc.Messages.First().Signals.Last().ValueTableMap.Count, Is.EqualTo(1));
            Assert.That(dbc.Messages.First().Signals.Last().ValueTableMap.Last().Key, Is.EqualTo(16384));
            Assert.That(dbc.Messages.First().Signals.Last().ValueTableMap.Last().Value, Is.EqualTo("ZERO_ANGLE"));
        }

        [Test]
        public void CommentWithTerminationAndMultiDefinitionInLine()
        {
            var dbcString = @"
BO_ 1160 DAS_steeringControl: 4 NEO
 SG_ DAS_steeringControlType : 23|2@0+ (1,0) [0|0] """"  EPAS
 SG_ DAS_steeringAngleRequest : 0|16@0+ (1,0) [0|0] """"  EPAS
 
CM_ BO_ 1160 ""This is a very fine comment; Right?""; VAL_ 1160 DAS_steeringControlType 1 ""ANGLE_CONTROL"" 3 ""DISABLED"" 0 ""NONE"" 2 ""RESERVED""; 
VAL_ 1160 DAS_steeringAngleRequest 16384 ""ZERO_ANGLE""; ";


            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.That(errorList, Has.Count.EqualTo(0));
            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
            Assert.That(dbc.Messages.First().Comment, Is.EqualTo("This is a very fine comment; Right?"));

            Assert.That(dbc.Messages.First().Signals.Count(), Is.EqualTo(2));
            Assert.That(dbc.Messages.First().Signals.First().ValueTableMap.Count, Is.EqualTo(4));
            Assert.That(dbc.Messages.First().Signals.First().ValueTableMap.Last().Key, Is.EqualTo(2));
            Assert.That(dbc.Messages.First().Signals.First().ValueTableMap.Last().Value, Is.EqualTo("RESERVED"));
            Assert.That(dbc.Messages.First().Signals.Last().ValueTableMap.Count, Is.EqualTo(1));
            Assert.That(dbc.Messages.First().Signals.Last().ValueTableMap.Last().Key, Is.EqualTo(16384));
            Assert.That(dbc.Messages.First().Signals.Last().ValueTableMap.Last().Value, Is.EqualTo("ZERO_ANGLE"));
        }
    }
}