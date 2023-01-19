using NUnit.Framework;
using System.IO;
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
                IsSigned = 1,
                ByteOrder = 1, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
                Factor = 0.01,
                Offset = 20
            };

            ulong TxMsg = Packer.TxSignalPack(-34.3, sig);
            Assert.AreEqual(43816, TxMsg);

            double val = Packer.RxSignalUnpack(TxMsg, sig);
            Assert.AreEqual(-34.3, val, 1e-2);
        }

        [Test]
        public void SimplePackingTestNonSigned()
        {
            var sig = new Signal
            {
                Length = 16,
                StartBit = 24,
                IsSigned = 1,
                ByteOrder = 1, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
                Factor = 0.125,
                Offset = 0
            };

            ulong TxMsg = Packer.TxSignalPack(800, sig);
            Assert.AreEqual(107374182400, TxMsg);

            double val = Packer.RxSignalUnpack(TxMsg, sig);
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
                IsSigned = 1,
                ByteOrder = 1, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
                Factor = 1e-16,
                Offset = 0
            };

            ulong TxMsg = Packer.TxSignalPack(396.31676720860366, sig);
            Assert.AreEqual(3963167672086036480, TxMsg);

            double val = Packer.RxSignalUnpack(TxMsg, sig);
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
                IsSigned = 0,
                ByteOrder = 1, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
                Factor = 1,
                Offset = -125
            };

            ulong TxMsg = Packer.TxSignalPack(8, sig);
            Assert.AreEqual(9583660007044415488, TxMsg);

            double val = Packer.RxSignalUnpack(TxMsg, sig);
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
                IsSigned = 0,
                ByteOrder = 1, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
                Factor = 1,
                Offset = 0
            };

            ulong TxMsg = Packer.TxSignalPack(1, sig);
            Assert.AreEqual(262144, TxMsg);

            double val = Packer.RxSignalUnpack(TxMsg, sig);
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
                IsSigned = 0,
                ByteOrder = 1, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
                Factor = 1,
                Offset = 0
            };

            ulong TxMsg = Packer.TxSignalPack(6, sig);
            Assert.AreEqual(384, TxMsg);

            double val = Packer.RxSignalUnpack(TxMsg, sig);
            Assert.AreEqual(6, val);

            val = Packer.RxSignalUnpack(498806260540323729, sig);
            Assert.AreEqual(6, val);
        }
    }
}