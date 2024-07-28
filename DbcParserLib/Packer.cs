using uint8_T = System.Byte;
using int64_T = System.Int64;
using uint64_T = System.UInt64;
using System;
using System.Runtime.InteropServices;
using DbcParserLib.Model;

namespace DbcParserLib
{
    public static class Packer
    {
        /// <summary>
        /// Function to pack a signal into a CAN data message
        /// </summary>
        /// <param name="value">Value to be packed</param>
        /// <param name="signal">Signal containing dbc information</param>
        /// <returns>Returns a 64 bit unsigned data message</returns>
        public static uint64_T TxSignalPack(double value, Signal signal)
        {
            int64_T iVal;
            uint64_T bitMask = signal.BitMask();

            // Apply scaling
            var rawValue = (value - signal.Offset) / signal.Factor;
            if (signal.ValueType == DbcValueType.IEEEFloat)
                iVal = (long)FloatConverter.AsInteger((float)rawValue);
            else if(signal.ValueType == DbcValueType.IEEEDouble)
                iVal = DoubleConverter.AsInteger(rawValue);
            else
                iVal = (int64_T)Math.Round(rawValue);

            // Apply overflow protection
            if (signal.ValueType == DbcValueType.Signed)
                iVal = CLAMP(iVal, -(int64_T)(bitMask >> 1) - 1, (int64_T)(bitMask >> 1));
            else if(signal.ValueType == DbcValueType.Unsigned)
                iVal = CLAMP(iVal, 0L, (int64_T)bitMask);

            // Manage sign bit (if signed)
            if (signal.ValueType == DbcValueType.Signed && iVal < 0)
            {
                iVal += (int64_T)(1UL << signal.Length);
            }

            // Pack signal
            if (signal.Intel()) // Little endian (Intel)
                return (((uint64_T)iVal & bitMask) << signal.StartBit);
            else // Big endian (Motorola)
                return MirrorMsg(((uint64_T)iVal & bitMask) << GetStartBitLE(signal));
        }

        /// <summary>
        /// Function to pack a state (unsigned integer) into a CAN data message
        /// </summary>
        /// <param name="value">Value to be packed</param>
        /// <param name="signal">Signal containing dbc information</param>
        /// <returns>Returns a 64 bit unsigned data message</returns>
        public static uint64_T TxStatePack(uint64_T value, Signal signal)
        {
            uint64_T bitMask = signal.BitMask();

            // Apply overflow protection
            value = CLAMP(value, 0UL, bitMask);

            // Pack signal
            if (signal.Intel()) // Little endian (Intel)
                return ((value & bitMask) << signal.StartBit);
            else // Big endian (Motorola)
                return MirrorMsg((value & bitMask) << GetStartBitLE(signal));
        }

        /// <summary>
        /// Function to unpack a signal from a CAN data message
        /// </summary>
        /// <param name="RxMsg64">The 64 bit unsigned data message</param>
        /// <param name="signal">Signal containing dbc information</param>
        /// <returns>Returns a double value representing the unpacked signal</returns>
        public static double RxSignalUnpack(uint64_T RxMsg64, Signal signal)
        {
            uint64_T iVal;
            uint64_T bitMask = signal.BitMask();

            // Unpack signal
            if (signal.Intel()) // Little endian (Intel)
                iVal = ((RxMsg64 >> signal.StartBit) & bitMask);
            else // Big endian (Motorola)
                iVal = ((MirrorMsg(RxMsg64) >> GetStartBitLE(signal)) & bitMask);

            return ApplySignAndScale(signal, iVal);
        }

        /// <summary>
        /// Function to unpack a state (unsigned integer) from a CAN data message
        /// </summary>
        /// <param name="RxMsg64">The 64 bit unsigned data message</param>
        /// <param name="signal">Signal containing dbc information</param>
        /// <returns>Returns an unsigned integer representing the unpacked state</returns>
        public static uint64_T RxStateUnpack(uint64_T RxMsg64, Signal signal)
        {
            uint64_T iVal;
            uint64_T bitMask = signal.BitMask();

            // Unpack signal
            if (signal.Intel()) // Little endian (Intel)
                iVal = (RxMsg64 >> signal.StartBit) & bitMask;
            else // Big endian (Motorola)
                iVal = (MirrorMsg(RxMsg64) >> GetStartBitLE(signal)) & bitMask;

            return iVal;
        }

        /// <summary>
        /// Function to unpack a signal from a CAN data message
        /// </summary>
        /// <param name="receiveMessage">The message data</param>
        /// <param name="signal">Signal containing dbc information</param>
        /// <returns>Returns a double value representing the unpacked signal</returns>
        public static double RxSignalUnpack(byte[] receiveMessage, Signal signal)
        {
            var startBit = signal.StartBit;
            var message = receiveMessage;

            if (!signal.Intel())
            {
                var copyArray = new byte[message.Length];
                Array.Copy(message, copyArray, message.Length);
                Array.Reverse(copyArray);

                message = copyArray;
                startBit = GetStartBitLE(signal, message.Length);
            }

            var iVal = ExtractBits(message, startBit, signal.Length);

            return ApplySignAndScale(signal, iVal);
        }

        private static double ApplySignAndScale(Signal signal, ulong value)
        {
            switch (signal.ValueType)
            {
                case DbcValueType.Signed:
                    int64_T signedValue;
                    if (signal.Length == 64)
                    {
                        signedValue = unchecked((long)value);
                    }
                    else
                    {
                        signedValue = Convert.ToInt64(value);
                        if ((value >> (signal.Length - 1)) != 0)
                        {
                            signedValue -= (1L << signal.Length);
                        }
                    }
                    return (double)(signedValue * (decimal)signal.Factor + (decimal)signal.Offset);
                case DbcValueType.IEEEFloat:
                    return FloatConverter.AsFloatingPoint((int)value) * signal.Factor + signal.Offset;
                case DbcValueType.IEEEDouble:
                    return DoubleConverter.AsFloatingPoint(unchecked((long)value)) * signal.Factor + signal.Offset;
                default:
                    return (double)(value * (decimal)signal.Factor + (decimal)signal.Offset);
            }            
        }

        private static int64_T CLAMP(int64_T x, int64_T low, int64_T high)
        {
            return Math.Max(low, Math.Min(x, high));
        }

        private static uint64_T CLAMP(uint64_T x, uint64_T low, uint64_T high)
        {
            return Math.Max(low, Math.Min(x, high));
        }

        /// <summary>
        /// Mirror data message. It is used to convert Big endian to Little endian and vice-versa
        /// </summary>
        private static uint64_T MirrorMsg(uint64_T msg)
        {
            uint8_T[] v =
            {
                (uint8_T)msg,
                (uint8_T)(msg >> 8),
                (uint8_T)(msg >> 16),
                (uint8_T)(msg >> 24),
                (uint8_T)(msg >> 32),
                (uint8_T)(msg >> 40),
                (uint8_T)(msg >> 48),
                (uint8_T)(msg >> 56)
            };
            return (((uint64_T)v[0] << 56)
                    | ((uint64_T)v[1] << 48)
                    | ((uint64_T)v[2] << 40)
                    | ((uint64_T)v[3] << 32)
                    | ((uint64_T)v[4] << 24)
                    | ((uint64_T)v[5] << 16)
                    | ((uint64_T)v[6] << 8)
                    | (uint64_T)v[7]);
        }

        /// <summary>
        /// Get start bit Little Endian
        /// </summary>
        private static uint8_T GetStartBitLE(Signal signal, int messageByteCount = 8)
        {
            uint8_T startByte = (uint8_T)(signal.StartBit / 8);
            return (uint8_T)(8 * messageByteCount - (signal.Length + 8 * startByte + (8 * (startByte + 1) - (signal.StartBit + 1)) % 8));
        }

        private static ulong ExtractBits(byte[] data, int startBit, int length)
        {
            ulong result = 0;
            int bitIndex = 0;

            for (int bitPos = startBit; bitPos < startBit + length; bitPos++)
            {
                int bytePos = bitPos / 8;
                int bitInByte = bitPos % 8;

                if (bytePos >= data.Length)
                    break;

                bool bit = (data[bytePos] & (1 << bitInByte)) != 0;
                if (bit)
                {
                    result |= 1UL << bitIndex;
                }

                bitIndex++;
            }

            return result;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public class FloatConverter
    {
        [FieldOffset(0)] public int Integer;
        [FieldOffset(0)] public float Float;

        public static int AsInteger(float value)
        {
            return new FloatConverter() { Float = value }.Integer;
        }

        public static float AsFloatingPoint(int value)
        {
            return new FloatConverter() { Integer = value }.Float;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public class DoubleConverter
    {
        [FieldOffset(0)] public long Integer;
        [FieldOffset(0)] public double Float;

        public static long AsInteger(double value)
        {
            return new DoubleConverter() { Float = value }.Integer;
        }

        public static double AsFloatingPoint(long value)
        {
            return new DoubleConverter() { Integer = value }.Float;
        }
    }
}
