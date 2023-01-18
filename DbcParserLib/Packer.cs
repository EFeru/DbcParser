using int8_T = System.SByte;
using uint8_T = System.Byte;
using int16_T = System.Int16;
using uint16_T = System.UInt16;
using int32_T = System.Int32;
using uint32_T = System.UInt32;
using int64_T = System.Int64;
using uint64_T = System.UInt64;
using System;
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
            uint64_T bitMask = (ulong.MaxValue >> (64 - signal.Length));

            // Apply scaling
            iVal = (int64_T)Math.Round((value - signal.Offset) / signal.Factor);
    
            // Apply overflow protection
            if (signal.IsSigned != 0)
                iVal = CLAMP(iVal, -(int64_T)(bitMask >> 1) - 1, (int64_T)(bitMask >> 1));
            else
                iVal = CLAMP(iVal, 0L, (int64_T)bitMask);

            // Manage sign bit (if signed)
            if (signal.IsSigned != 0 && iVal < 0) {
              iVal += (int64_T)(1UL << signal.Length);
            }

            // Pack signal
            if (signal.ByteOrder != 0)  // Little endian (Intel)
                return (((uint64_T)iVal & bitMask) << signal.StartBit);
            else                        // Big endian (Motorola)
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
            uint64_T bitMask = (ulong.MaxValue >> (64 - signal.Length));
    
            // Apply overflow protection
            value = CLAMP(value, 0UL, bitMask);
    
            // Pack signal
            if (signal.ByteOrder != 0)  // Little endian (Intel)
                return ((value & bitMask) << signal.StartBit);
            else                        // Big endian (Motorola)
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
            int64_T iVal; 
            uint64_T bitMask = (ulong.MaxValue >> (64 - signal.Length));

            // Unpack signal
            if (signal.ByteOrder != 0)  // Little endian (Intel)
                iVal = (int64_T)((RxMsg64 >> signal.StartBit) & bitMask);
            else                        // Big endian (Motorola)
                iVal = (int64_T)((MirrorMsg(RxMsg64) >> GetStartBitLE(signal)) & bitMask);

            // Manage sign bit (if signed)
            if (signal.IsSigned != 0) {
              iVal -= ((iVal >> (signal.Length - 1)) != 0) ? (1L << signal.Length) : 0L;
            }

            // Apply scaling
            return ((double)iVal * signal.Factor + signal.Offset);
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
            uint64_T bitMask = (ulong.MaxValue >> (64 - signal.Length));

            // Unpack signal
            if (signal.ByteOrder != 0)  // Little endian (Intel)
                iVal = (RxMsg64 >> signal.StartBit) & bitMask;
            else                        // Big endian (Motorola)
                iVal = (MirrorMsg(RxMsg64) >> GetStartBitLE(signal)) & bitMask;

            // Apply scaling
            return iVal;
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
            return   (((uint64_T)v[0] << 56)
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
        private static uint8_T GetStartBitLE(Signal signal)
        {
            uint8_T startByte = (uint8_T)(signal.StartBit / 8);
            return (uint8_T)(64 - (signal.Length + 8 * startByte + (8 * (startByte + 1) - (signal.StartBit + 1)) % 8));
        }
    }
}
