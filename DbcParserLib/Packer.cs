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
        public static ulong TxSignalPack(double value, Signal signal)
        {
            long iVal = TxPackApplySignAndScale(value, signal);
            ulong bitMask = signal.BitMask();

            // Pack signal
            if (signal.Intel()) // Little endian (Intel)
                return (((ulong)iVal & bitMask) << signal.StartBit);
            else // Big endian (Motorola)
                return MirrorMsg(((ulong)iVal & bitMask) << GetStartBitLE(signal));
        }

        /// <summary>
        /// Function to pack a signal into a CAN data message
        /// </summary>
        /// <param name="message">A ref to the byte array containing the message</param>
        /// <param name="value">Value to be packed</param>
        /// <param name="signal">Signal containing dbc information</param>
        /// <remarks>Due to needing to reverse the byte array when handling BigEndian(Motorola) format this method can not be called in parallel for multiple signals in one message.
        /// To make this obvios the message is a ref and is actually reassigned after handling BigEndian format.</remarks>
        public static void TxSignalPack(ref byte[] message, double value, Signal signal)
        {
            long iVal = TxPackApplySignAndScale(value, signal);

            // Pack signal
            if (!signal.Intel())
            {
                var tempArray = new byte[message.Length];
                Array.Copy(message, tempArray, message.Length);
                Array.Reverse(tempArray);

                WriteBits(tempArray, unchecked((ulong)iVal), GetStartBitLE(signal, message.Length), signal.Length);

                Array.Reverse(tempArray);

                message = tempArray;
                return;
            }
            WriteBits(message, unchecked((ulong)iVal), signal.StartBit, signal.Length);
        }

        /// <summary>
        /// Function to pack a state (unsigned integer) into a CAN data message
        /// </summary>
        /// <param name="value">Value to be packed</param>
        /// <param name="signal">Signal containing dbc information</param>
        /// <returns>Returns a 64 bit unsigned data message</returns>
        public static ulong TxStatePack(ulong value, Signal signal)
        {
            ulong bitMask = signal.BitMask();

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
        public static double RxSignalUnpack(ulong RxMsg64, Signal signal)
        {
            ulong iVal;
            ulong bitMask = signal.BitMask();

            // Unpack signal
            if (signal.Intel()) // Little endian (Intel)
                iVal = ((RxMsg64 >> signal.StartBit) & bitMask);
            else // Big endian (Motorola)
                iVal = ((MirrorMsg(RxMsg64) >> GetStartBitLE(signal)) & bitMask);

            return RxUnpackApplySignAndScale(signal, iVal);
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
                var tempArray = new byte[message.Length];
                Array.Copy(message, tempArray, message.Length);
                Array.Reverse(tempArray);

                message = tempArray;
                startBit = GetStartBitLE(signal, message.Length);
            }

            var iVal = ExtractBits(message, startBit, signal.Length);

            return RxUnpackApplySignAndScale(signal, iVal);
        }

        /// <summary>
        /// Function to unpack a state (unsigned integer) from a CAN data message
        /// </summary>
        /// <param name="RxMsg64">The 64 bit unsigned data message</param>
        /// <param name="signal">Signal containing dbc information</param>
        /// <returns>Returns an unsigned integer representing the unpacked state</returns>
        public static ulong RxStateUnpack(ulong RxMsg64, Signal signal)
        {
            ulong iVal;
            ulong bitMask = signal.BitMask();

            // Unpack signal
            if (signal.Intel()) // Little endian (Intel)
                iVal = (RxMsg64 >> signal.StartBit) & bitMask;
            else // Big endian (Motorola)
                iVal = (MirrorMsg(RxMsg64) >> GetStartBitLE(signal)) & bitMask;

            return iVal;
        }

        private static long TxPackApplySignAndScale(double value, Signal signal)
        {
            long iVal;
            ulong bitMask = signal.BitMask();

            // Apply scaling
            var rawValue = (value - signal.Offset) / signal.Factor;
            if (signal.ValueType == DbcValueType.IEEEFloat)
                iVal = FloatConverter.AsInteger((float)rawValue);
            else if (signal.ValueType == DbcValueType.IEEEDouble)
                iVal = DoubleConverter.AsInteger(rawValue);
            else
                iVal = (long)Math.Round(rawValue);

            // Apply overflow protection
            if (signal.ValueType == DbcValueType.Signed)
                iVal = CLAMP(iVal, -(long)(bitMask >> 1) - 1, (long)(bitMask >> 1));
            else if (signal.ValueType == DbcValueType.Unsigned)
                iVal = CLAMP(iVal, 0L, (long)bitMask);

            // Manage sign bit (if signed)
            if (signal.ValueType == DbcValueType.Signed && iVal < 0)
            {
                iVal += (long)(1UL << signal.Length);
            }

            return iVal;
        }

        private static double RxUnpackApplySignAndScale(Signal signal, ulong value)
        {
            switch (signal.ValueType)
            {
                case DbcValueType.Signed:
                    long signedValue;
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

        private static long CLAMP(long x, long low, long high)
        {
            return Math.Max(low, Math.Min(x, high));
        }

        private static ulong CLAMP(ulong x, ulong low, ulong high)
        {
            return Math.Max(low, Math.Min(x, high));
        }

        /// <summary>
        /// Mirror data message. It is used to convert Big endian to Little endian and vice-versa
        /// </summary>
        private static ulong MirrorMsg(ulong msg)
        {
            byte[] v =
            {
                (byte)msg,
                (byte)(msg >> 8),
                (byte)(msg >> 16),
                (byte)(msg >> 24),
                (byte)(msg >> 32),
                (byte)(msg >> 40),
                (byte)(msg >> 48),
                (byte)(msg >> 56)
            };
            return (((ulong)v[0] << 56)
                    | ((ulong)v[1] << 48)
                    | ((ulong)v[2] << 40)
                    | ((ulong)v[3] << 32)
                    | ((ulong)v[4] << 24)
                    | ((ulong)v[5] << 16)
                    | ((ulong)v[6] << 8)
                    | (ulong)v[7]);
        }

        /// <summary>
        /// Get start bit Little Endian
        /// </summary>
        private static byte GetStartBitLE(Signal signal, int messageByteCount = 8)
        {
            byte startByte = (byte)(signal.StartBit / 8);
            return (byte)(8 * messageByteCount - (signal.Length + 8 * startByte + (8 * (startByte + 1) - (signal.StartBit + 1)) % 8));
        }

        private static void WriteBits(byte[] data, ulong value, int startBit, int length)
        {
            for (int bitIndex = 0; bitIndex < length; bitIndex++)
            {
                int bitPosition = startBit + bitIndex;
                int byteIndex = bitPosition / 8;
                int bitInBytePosition = bitPosition % 8;

                // Extract the bit from the signal value
                ulong bitValue = (value >> bitIndex) & 1;

                // Set the bit in the message
                data[byteIndex] |= (byte)(bitValue << bitInBytePosition);
            }
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
