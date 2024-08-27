using System.Linq;
using DbcParserLib.Model;
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

            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(4, dbc.Messages.SelectMany(m => m.Signals).Count());

            var signal1 = dbc.Messages.First().Signals.First(x => x.Name.Equals("S1_PID_0D_VehicleSpeed"));
            var signal2 = dbc.Messages.First().Signals.First(x => x.Name.Equals("S1_PID_11_ThrottlePosition"));
            var signal3 = dbc.Messages.First().Signals.First(x => x.Name.Equals("S1"));
            var signal4 = dbc.Messages.First().Signals.First(x => x.Name.Equals("Service"));

            Assert.AreEqual("S1 13-13", signal1.ExtendedMultiplexing);
            Assert.AreEqual("S1 17-17", signal2.ExtendedMultiplexing);
            Assert.AreEqual("Service 1-1", signal3.ExtendedMultiplexing);
            Assert.AreEqual(string.Empty, signal4.ExtendedMultiplexing);
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

            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(4, dbc.Messages.SelectMany(m => m.Signals).Count());

            var signal1 = dbc.Messages.First().Signals.First(x => x.Name.Equals("Mux_1"));
            var signal2 = dbc.Messages.First().Signals.First(x => x.Name.Equals("Mux_2"));
            var signal3 = dbc.Messages.First().Signals.First(x => x.Name.Equals("Mux_3"));
            var signal4 = dbc.Messages.First().Signals.First(x => x.Name.Equals("Mux_4"));

            Assert.AreEqual(string.Empty, signal1.ExtendedMultiplexing);
            Assert.AreEqual("Mux_1 3-3, 5-10", signal2.ExtendedMultiplexing);
            Assert.AreEqual("Mux_2 3-3", signal3.ExtendedMultiplexing);
            Assert.AreEqual("Mux_3 2-2", signal4.ExtendedMultiplexing);
        }
    }
}
