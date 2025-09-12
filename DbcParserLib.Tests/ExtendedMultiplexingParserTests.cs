using DbcParserLib.Observers;
using NUnit.Framework;
using System;
using System.Linq;


namespace DbcParserLib.Tests
{
    internal class ExtendedMultiplexingParserTests
    {
        [Test]
        public void ParseSignalExtendedMultiplexingCase1()
        {
            var dbcString = @"
BO_ 2024 OBD2: 8 Vector__XXX
    SG_ S1_PID_0D_VehicleSpeed m13 : 31|8@0+ (1,0) [0|255] ""km/h"" Vector__XXX
    SG_ S1_PID_11_ThrottlePosition m17 : 31|8@0+ (0.39216,0) [0|100] ""%"" Vector__XXX
    SG_ S1 m1M : 23|8@0+ (1,0) [0|255] """" Vector__XXX
    SG_ Service M : 11|4@0+ (1,0) [0|15] """" Vector__XXX

SG_MUL_VAL_ 2024 S1_PID_0D_VehicleSpeed S1 13-13;
SG_MUL_VAL_ 2024 S1_PID_11_ThrottlePosition S1 17-17;
SG_MUL_VAL_ 2024 S1 Service 1-1;";

            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));

            var message = dbc.Messages.First();

            Assert.That(message.Signals, Has.Count.EqualTo(4));

            Assert.That(errorList, Has.Count.EqualTo(0));

            // ToDo: When multiplexing is in the model data it needs to be validated here
        }

        [Test]
        public void ParseSignalExtendedMultiplexingCase2()
        {
            var dbcString = @"
BO_ 100 MuxMsg: 1 Vector__XXX 
    SG_ Mux_4 m2 : 6|2@1+ (1,0) [0|0] """" Vector__XXX 
    SG_ Mux_3 m3M : 4|2@1+ (1,0) [0|0] """" Vector__XXX 
    SG_ Mux_2 m3M : 2|2@1+ (1,0) [0|0] """" Vector__XXX 
    SG_ Mux_1 M : 0|2@1+ (1,0) [0|0] """" Vector__XXX
    
SG_MUL_VAL_ 100 Mux_2 Mux_1 3-3, 5-10;
SG_MUL_VAL_ 100 Mux_3 Mux_2 3-3;
SG_MUL_VAL_ 100 Mux_4 Mux_3 2-2;";

            var failureObserver = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(failureObserver);
            var dbc = Parser.Parse(dbcString);
            var errorList = failureObserver.GetErrorList();

            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));

            var message = dbc.Messages.First();

            Assert.That(message.Signals, Has.Count.EqualTo(4));

            Assert.That(errorList, Has.Count.EqualTo(0));

            // ToDo: When multiplexing is in the model data it needs to be validated here
        }

        [TestCase("SG_MUL_VAL_ abc Mux_2 Mux_1 3-3 5-10;")]
        [TestCase("SG_MUL_VAL_ abc Mux_2 Mux_1 3-3, 5-10")]
        [TestCase("SG_MUL_VAL_ abc Mux_2 Mux_1 3-3, 5-10;")]
        [TestCase("SG_MUL_VAL_ 100 Mux_2Mux_1 3-3, 5-10;")]
        [TestCase("SG_MUL_VAL_ 100 Mux_2 Mux_1 3-a;")]
        public void ParseExtendedMultiplexingErrorIsObserved(string dbcString)
        {
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