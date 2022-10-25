using NUnit.Framework;
using System.IO;
using DbcParserLib.Model;

namespace DbcParserLib.Tests
{
    public class PackerTests
    {
        [Test]
        public void PackingTest()
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
    }
}