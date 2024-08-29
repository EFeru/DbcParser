using System.Linq;
using NUnit.Framework;


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


            var dbc = Parser.Parse(dbcString);

            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));

            var message = dbc.Messages.First();

            Assert.That(message.Signals, Has.Count.EqualTo(4));

            var signal1 = message.Signals.FirstOrDefault(x => x.Name.Equals("S1_PID_0D_VehicleSpeed"));
            var signal2 = message.Signals.FirstOrDefault(x => x.Name.Equals("S1_PID_11_ThrottlePosition"));
            var signal3 = message.Signals.FirstOrDefault(x => x.Name.Equals("S1"));
            var signal4 = message.Signals.FirstOrDefault(x => x.Name.Equals("Service"));

            Assert.Multiple(() =>
            {
                Assert.That(signal1?.ExtendedMultiplexing, Is.EqualTo("S1 13-13"));
                Assert.That(signal2?.ExtendedMultiplexing, Is.EqualTo("S1 17-17"));
                Assert.That(signal3?.ExtendedMultiplexing, Is.EqualTo("Service 1-1"));
                Assert.That(signal4, Is.Not.Null);
                Assert.That(signal4?.ExtendedMultiplexing, Is.Null);
            });
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


            var dbc = Parser.Parse(dbcString);

            Assert.That(dbc.Messages.Count(), Is.EqualTo(1));

            var message = dbc.Messages.First();

            Assert.That(message.Signals, Has.Count.EqualTo(4));

            var signal1 = message.Signals.FirstOrDefault(x => x.Name.Equals("Mux_1"));
            var signal2 = message.Signals.FirstOrDefault(x => x.Name.Equals("Mux_2"));
            var signal3 = message.Signals.FirstOrDefault(x => x.Name.Equals("Mux_3"));
            var signal4 = message.Signals.FirstOrDefault(x => x.Name.Equals("Mux_4"));

            Assert.Multiple(() =>
            {
                Assert.That(signal1, Is.Not.Null);
                Assert.That(signal1?.ExtendedMultiplexing, Is.Null);
                Assert.That(signal2?.ExtendedMultiplexing, Is.EqualTo("Mux_1 3-3, 5-10"));
                Assert.That(signal3?.ExtendedMultiplexing, Is.EqualTo("Mux_2 3-3"));
                Assert.That(signal4?.ExtendedMultiplexing, Is.EqualTo("Mux_3 2-2"));
            });
        }
    }
}
