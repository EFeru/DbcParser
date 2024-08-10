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

        private Signal EightByte_LittleEndian_Signal1;
        private Signal EightByte_LittleEndian_Signal2;
        private Signal EightByte_LittleEndian_Signal3;
        private Signal EightByte_LittleEndian_Signal4;

        private Signal LittleEndian_Unsigned_NoScale;

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

            EightByte_LittleEndian_Signal1 = new Signal
            {
                StartBit = 0,
                Length = 10,
                ValueType = DbcValueType.Unsigned,
                ByteOrder = 1,
                Factor = 1,
                Offset = 0
            };

            EightByte_LittleEndian_Signal2 = new Signal
            {
                StartBit = 10,
                Length = 6,
                ValueType = DbcValueType.Unsigned,
                ByteOrder = 1,
                Factor = 1,
                Offset = 0
            };

            EightByte_LittleEndian_Signal3 = new Signal
            {
                StartBit = 16,
                Length = 32,
                ValueType = DbcValueType.Unsigned,
                ByteOrder = 1,
                Factor = 1,
                Offset = 0
            };

            EightByte_LittleEndian_Signal4 = new Signal
            {
                StartBit = 48,
                Length = 16,
                ValueType = DbcValueType.Unsigned,
                ByteOrder = 1,
                Factor = 1,
                Offset = 0
            };

            LittleEndian_Unsigned_NoScale = new Signal
            {
                StartBit = 0,
                Length = 32,
                ValueType = DbcValueType.Unsigned,
                ByteOrder = 1,
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
        public byte[] Pack_8Byte_BigEndian_ByteArray()
        {
            byte[] TxMsg = new byte[8];
            Packer.TxSignalPack(TxMsg, 0, EightByte_BigEndian_Signal1);
            Packer.TxSignalPack(TxMsg, 63, EightByte_BigEndian_Signal2);
            Packer.TxSignalPack(TxMsg, 0, EightByte_BigEndian_Signal3);
            Packer.TxSignalPack(TxMsg, ushort.MaxValue, EightByte_BigEndian_Signal4);
            return TxMsg;
        }

        [Benchmark]
        public ulong Pack_8Byte_LittleEndian_Uint64()
        {
            ulong TxMsg = 0;
            TxMsg |= Packer.TxSignalPack(0, EightByte_LittleEndian_Signal1);
            TxMsg |= Packer.TxSignalPack(63, EightByte_LittleEndian_Signal2);
            TxMsg |= Packer.TxSignalPack(0, EightByte_LittleEndian_Signal3);
            TxMsg |= Packer.TxSignalPack(ushort.MaxValue, EightByte_LittleEndian_Signal4);
            return TxMsg;
        }

        [Benchmark]
        public byte[] Pack_8Byte_LittleEndian_ByteArray()
        {
            byte[] TxMsg = new byte[8];
            Packer.TxSignalPack(TxMsg, 0, EightByte_LittleEndian_Signal1);
            Packer.TxSignalPack(TxMsg, 63, EightByte_LittleEndian_Signal2);
            Packer.TxSignalPack(TxMsg, 0, EightByte_LittleEndian_Signal3);
            Packer.TxSignalPack(TxMsg, ushort.MaxValue, EightByte_LittleEndian_Signal4);
            return TxMsg;
        }

        [Benchmark]
        public ulong Pack_1Signal_Unsigned_NoScale_StatePack()
        {
            ulong TxMsg = 0;
            TxMsg |= Packer.TxStatePack(123, LittleEndian_Unsigned_NoScale);
            return TxMsg;
        }

        [Benchmark]
        public ulong Pack_1Signal_Unsigned_NoScale_SignalPack()
        {
            ulong TxMsg = 0;
            TxMsg |= Packer.TxSignalPack(123, LittleEndian_Unsigned_NoScale);
            return TxMsg;
        }

        [Benchmark]
        public byte[] Pack_1Signal_Unsigned_NoScale_SignalPackByteArray()
        {
            byte[] TxMsg = new byte[8];
            Packer.TxSignalPack(TxMsg, 123, LittleEndian_Unsigned_NoScale);
            return TxMsg;
        }
    }
}