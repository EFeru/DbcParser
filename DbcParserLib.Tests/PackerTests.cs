using NUnit.Framework;
using System.IO;
using DbcParserLib.Model;

namespace DbcParserLib.Tests
{
    public class PackerTests
    {
        [Test]
        public void SimplePackingTest()
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
    }
}
