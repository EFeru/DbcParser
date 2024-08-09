using DbcParserLib.Model;
using BenchmarkDotNet.Attributes;

namespace DbcParserLib.Tests
{
    public class PackerBenchmark
    {
        private Signal EightByte_BigEndian_Signal1;
        private Signal EightByte_BigEndian_Signal2;
        private Signal EightByte_BigEndian_Signal3;
        private Signal EightByte_BigEndian_Signal4;

        [GlobalSetup]
        public void SetupSignals()
        {
            EightByte_BigEndian_Signal1 = new Signal
            {
                StartBit = 7,
                Length = 10,
                ValueType = DbcValueType.Unsigned,
                ByteOrder = 0,
                Factor = 1,
                Offset = 0
            };

            EightByte_BigEndian_Signal2 = new Signal
            {
                StartBit = 13,
                Length = 6,
                ValueType = DbcValueType.Unsigned,
                ByteOrder = 0,
                Factor = 1,
                Offset = 0
            };

            EightByte_BigEndian_Signal3 = new Signal
            {
                StartBit = 23,
                Length = 32,
                ValueType = DbcValueType.Unsigned,
                ByteOrder = 0,
                Factor = 1,
                Offset = 0
            };

            EightByte_BigEndian_Signal4 = new Signal
            {
                StartBit = 55,
                Length = 16,
                ValueType = DbcValueType.Unsigned,
                ByteOrder = 0,
                Factor = 1,
                Offset = 0
            };
        }

        [Benchmark]
        public ulong Pack_8Byte_BigEndian_Uint64()
        {
            ulong TxMsg = 0;
            TxMsg |= Packer.TxSignalPack(0, EightByte_BigEndian_Signal1);
            TxMsg |= Packer.TxSignalPack(63, EightByte_BigEndian_Signal2);
            TxMsg |= Packer.TxSignalPack(0, EightByte_BigEndian_Signal3);
            TxMsg |= Packer.TxSignalPack(ushort.MaxValue, EightByte_BigEndian_Signal4);
            return TxMsg;   
        }

        [Benchmark]
        public ulong Pack_8Byte_BigEndian_ByteArray()
        {
            ulong TxMsg = 0;
            TxMsg |= Packer.TxSignalPack(0, EightByte_BigEndian_Signal1);
            TxMsg |= Packer.TxSignalPack(63, EightByte_BigEndian_Signal2);
            TxMsg |= Packer.TxSignalPack(0, EightByte_BigEndian_Signal3);
            TxMsg |= Packer.TxSignalPack(ushort.MaxValue, EightByte_BigEndian_Signal4);
            return TxMsg;
        }
    }
}