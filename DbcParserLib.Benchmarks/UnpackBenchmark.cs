using DbcParserLib.Model;
using BenchmarkDotNet.Attributes;

namespace DbcParserLib.Tests
{
    public class UnpackBenchmark
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

        private ulong RxMsg = ulong.MaxValue;
        private byte[] RxBytes = new byte[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue };

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
        public void Unpack_8Byte_BigEndian_Uint64()
        {
            var signal1 = Packer.RxSignalUnpack(RxMsg, EightByte_BigEndian_Signal1);
            var signal2 = Packer.RxSignalUnpack(RxMsg, EightByte_BigEndian_Signal2);
            var signal3 = Packer.RxSignalUnpack(RxMsg, EightByte_BigEndian_Signal3);
            var signal4 = Packer.RxSignalUnpack(RxMsg, EightByte_BigEndian_Signal4);  
        }

        [Benchmark]
        public void Unpack_8Byte_BigEndian_ByteArray()
        {
            var signal1 = Packer.RxSignalUnpack(RxBytes, EightByte_BigEndian_Signal1);
            var signal2 = Packer.RxSignalUnpack(RxBytes, EightByte_BigEndian_Signal2);
            var signal3 = Packer.RxSignalUnpack(RxBytes, EightByte_BigEndian_Signal3);
            var signal4 = Packer.RxSignalUnpack(RxBytes, EightByte_BigEndian_Signal4);
        }

        [Benchmark]
        public void Unpack_8Byte_LittleEndian_Uint64()
        {
            var signal1 = Packer.RxSignalUnpack(RxMsg, EightByte_LittleEndian_Signal1);
            var signal2 = Packer.RxSignalUnpack(RxMsg, EightByte_LittleEndian_Signal2);
            var signal3 = Packer.RxSignalUnpack(RxMsg, EightByte_LittleEndian_Signal3);
            var signal4 = Packer.RxSignalUnpack(RxMsg, EightByte_LittleEndian_Signal4);
        }

        [Benchmark]
        public void Unpack_8Byte_LittleEndian_ByteArray()
        {
            var signal1 = Packer.RxSignalUnpack(RxBytes, EightByte_LittleEndian_Signal1);
            var signal2 = Packer.RxSignalUnpack(RxBytes, EightByte_LittleEndian_Signal2);
            var signal3 = Packer.RxSignalUnpack(RxBytes, EightByte_LittleEndian_Signal3);
            var signal4 = Packer.RxSignalUnpack(RxBytes, EightByte_LittleEndian_Signal4);
        }

        [Benchmark]
        public ulong Unpack_1Signal_Unsigned_NoScale_State()
        {
            return Packer.RxStateUnpack(RxMsg, LittleEndian_Unsigned_NoScale);
        }

        [Benchmark]
        public double Unpack_1Signal_Unsigned_NoScale_Signal()
        {
            return Packer.RxSignalUnpack(RxMsg, LittleEndian_Unsigned_NoScale);
        }

        [Benchmark]
        public double Unpack_1Signal_Unsigned_NoScale_SignalByteArray()
        {
            return Packer.RxSignalUnpack(RxBytes, LittleEndian_Unsigned_NoScale);
        }
    }
}