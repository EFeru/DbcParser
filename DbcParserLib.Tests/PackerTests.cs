using NUnit.Framework;
using DbcParserLib.Model;

namespace DbcParserLib.Tests
{
    public class PackerTests
    {
        [Test]
        public void SimplePackingTestSigned()
        {
            var sig = new Signal
            {
                Length = 14,
                StartBit = 2,
                ValueType = DbcValueType.Signed,
                ByteOrder = 1, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
                Factor = 0.01,
                Offset = 20
            };

            var txMsg = Packer.TxSignalPack(-34.3, sig);
            Assert.AreEqual(43816, txMsg);

            var val = Packer.RxSignalUnpack(txMsg, sig);
            Assert.AreEqual(-34.3, val, 1e-2);
        }

        [TestCase((ushort)0, 3255382835ul)]
        [TestCase((ushort)2, 13021531340ul)]
        [TestCase((ushort)5, 104172250720ul)]
        [TestCase((ushort)12, 13334048092160ul)]
        public void FloatLittleEndianValuePackingTest(ushort start, ulong packet)
        {
            var sig = new Signal
            {
                Length = 32,
                StartBit = start,
                ValueType = DbcValueType.IEEEFloat,
                ByteOrder = 1, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
                Factor = 1,
                Offset = 0
            };

            var expected = -34.3f;
            var txMsg = Packer.TxSignalPack(expected, sig);
            Assert.AreEqual(packet, txMsg);

            var val = Packer.RxSignalUnpack(txMsg, sig);
            Assert.AreEqual(expected, val, 1e-2);
        }

        [TestCase((ushort)0, 439799153665ul)]
        [TestCase((ushort)2, 655406731270ul)]
        [TestCase((ushort)5, 828061286960ul)]
        [TestCase((ushort)12, 105991844730880ul)]
        public void FloatBigEndianValuePackingTest(ushort start, ulong packet)
        {
            var sig = new Signal
            {
                Length = 32,
                StartBit = start,
                ValueType = DbcValueType.IEEEFloat,
                ByteOrder = 0, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
                Factor = 1,
                Offset = 0
            };

            var value = -34.3f;
            var txMsg = Packer.TxSignalPack(value, sig);
            Assert.AreEqual(packet, txMsg);

            var val = Packer.RxSignalUnpack(txMsg, sig);
            Assert.AreEqual(value, val, 1e-2);
        }

        [Test]
        public void DoubleLittleEndianValuePackingTest()
        {
            var sig = new Signal
            {
                Length = 64,
                StartBit = 0,
                ValueType = DbcValueType.IEEEDouble,
                ByteOrder = 1, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
                Factor = 1,
                Offset = 0
            };

            var expected = -34.3567;
            var txMsg = Packer.TxSignalPack(expected, sig);
            Assert.AreEqual(13853404129830452697, txMsg);

            var val = Packer.RxSignalUnpack(txMsg, sig);
            Assert.AreEqual(expected, val, 1e-2);
        }

        [Test]
        public void DoubleBigEndianValuePackingTest()
        {
            var sig = new Signal
            {
                Length = 64,
                StartBit = 7,
                ValueType = DbcValueType.IEEEDouble,
                ByteOrder = 0, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
                Factor = 1,
                Offset = 0
            };

            var expected = -34.35564;
            var txMsg = Packer.TxSignalPack(expected, sig);
            Assert.AreEqual(2419432028705210816, txMsg);

            var val = Packer.RxSignalUnpack(txMsg, sig);
            Assert.AreEqual(expected, val, 1e-2);
        }

        [Test]
        public void SimplePackingTestNonSigned()
        {
            var sig = new Signal
            {
                Length = 16,
                StartBit = 24,
                ValueType = DbcValueType.Signed,
                ByteOrder = 1, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
                Factor = 0.125,
                Offset = 0
            };

            var txMsg = Packer.TxSignalPack(800, sig);
            Assert.AreEqual(107374182400, txMsg);

            var val = Packer.RxSignalUnpack(txMsg, sig);
            Assert.AreEqual(800, val);

            val = Packer.RxSignalUnpack(9655716608953581040, sig);
            Assert.AreEqual(800, val);
        }

        [Test]
        public void PackingTest64Bit()
        {
            var sig = new Signal
            {
                Length = 64,
                StartBit = 0,
                ValueType = DbcValueType.Signed,
                ByteOrder = 1, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
                Factor = 1e-16,
                Offset = 0
            };

            var txMsg = Packer.TxSignalPack(396.31676720860366, sig);
            Assert.AreEqual(3963167672086036480, txMsg);

            var val = Packer.RxSignalUnpack(txMsg, sig);
            Assert.AreEqual(396.31676720860366, val);
        }

        //Although Pack has one output per value/signal. Unpack can produce the same result for two different RxMsg64 inputs
        [Test]
        public void UnPackingTestMultipleUnpacks()
        {
            var sig = new Signal
            {
                Length = 8,
                StartBit = 56,
                ValueType = DbcValueType.Unsigned,
                ByteOrder = 1, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
                Factor = 1,
                Offset = -125
            };

            var txMsg = Packer.TxSignalPack(8, sig);
            Assert.AreEqual(9583660007044415488, txMsg);

            var val = Packer.RxSignalUnpack(txMsg, sig);
            Assert.AreEqual(8, val);

            val = Packer.RxSignalUnpack(9655716608953581040, sig);
            Assert.AreEqual(8, val);
        }

        //A bit packing test with a length of 1 (to test signals with < 8 bits)
        [Test]
        public void BitPackingTest1()
        {
            var sig = new Signal
            {
                Length = 1,
                StartBit = 18,
                ValueType = DbcValueType.Unsigned,
                ByteOrder = 1, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
                Factor = 1,
                Offset = 0
            };

            var txMsg = Packer.TxSignalPack(1, sig);
            Assert.AreEqual(262144, txMsg);

            var val = Packer.RxSignalUnpack(txMsg, sig);
            Assert.AreEqual(1, val);

            val = Packer.RxSignalUnpack(140737488617472, sig);
            Assert.AreEqual(1, val);
        }

        //A bit packing test with a length of 3 (to test signals with < 8 bits)
        [Test]
        public void BitPackingTest2()
        {
            var sig = new Signal
            {
                Length = 3,
                StartBit = 6,
                ValueType = DbcValueType.Unsigned,
                ByteOrder = 1, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
                Factor = 1,
                Offset = 0
            };

            var txMsg = Packer.TxSignalPack(6, sig);
            Assert.AreEqual(384, txMsg);

            var val = Packer.RxSignalUnpack(txMsg, sig);
            Assert.AreEqual(6, val);

            val = Packer.RxSignalUnpack(498806260540323729, sig);
            Assert.AreEqual(6, val);
        }
    }
}