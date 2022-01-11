using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using int8_T = System.SByte;
using uint8_T = System.Byte;
using int16_T = System.Int16;
using uint16_T = System.UInt16;
using int32_T = System.Int32;
using uint32_T = System.UInt32;
using int64_T = System.Int64;
using uint64_T = System.UInt64;

/* 
 * ------------------------------------
 * Author:  Emanuel Feru
 * Year:    2022
 * 
 * Copyright (C) Emanuel Feru
 * ------------------------------------
 */

namespace DbcParserLib
{
    public class Message
    {
        public uint ID;
        public bool IsExtID;
        public string Name;
        public byte DLC;
        public string Transmitter;
        public int CycleTime;
        public List<Signal> Signals = new List<Signal>();
    }

    public class Signal
    {
        public uint ID;
        public string Name;
        public byte StartBit;
        public byte Length;
        public byte ByteOrder = 1;
        public byte IsSigned;
        public double InitialValue;
        public double Factor = 1;
        public double Offset;
        public double Minimum;
        public double Maximum;
        public string Unit;
        public string[] Receiver;
        public string ValueTable;
        public string Comment;
    }

    public class DbcParser
    {
        /// <summary>
        /// List with Nodes from a dbc
        /// </summary>
        public List<string> Nodes = new List<string>();
        /// <summary>
        /// List with Messages containing Signals from a dbc
        /// </summary>
        public List<Message> Messages = new List<Message>();

        /// <summary>
        /// Reads and parses the information from a *.dbc file. As input it expects the dbc file path as string.
        /// </summary>
        public void ReadFromFile(string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var textReader = new StreamReader(fileStream))
            {
                ReadFromString(textReader.ReadToEnd());
            }
        }

        /// <summary>
        /// Reads and parses the information from a string in a dbc format.
        /// </summary>
        public void ReadFromString(string dbcAsString)
        {
            try
            {
                string line;
                string[] data;

                // Parse for Windows new line "\r\n"
                data = dbcAsString.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                // Extra check in case the new line is "\n" for Unix or "\r" for Mac
                if (data.Length < 3)
                {
                    data = dbcAsString.Split(new string[] { "\n" }, StringSplitOptions.None);
                    if (data.Length < 3)
                        data = dbcAsString.Split(new string[] { "\r" }, StringSplitOptions.None);
                }

                for (int i = 0; i < data.Length; i++)
                {
                    line = data[i];
                    line = line.Trim();

                    // Extract Nodes
                    if (line.StartsWith("BU_: "))
                        AddNodes(line);

                    // Extract Messages
                    else if (line.StartsWith("BO_ "))
                        AddMessage(line);

                    // Extract Signals
                    else if (line.StartsWith("SG_ "))
                        AddSignal(Messages.Count - 1, Messages.Last().ID, line);

                    // Extract StartValue and CycleTime
                    else if (line.StartsWith("BA_ "))
                    {
                        if (line.Contains("GenMsgCycleTime\" BO_"))
                            SetCycleTime(line);
                        else if (line.Contains("GenSigStartValue\" SG_"))
                            SetInitialValue(line);
                    }

                    // Extract Signal Comment
                    else if (line.StartsWith("CM_ SG_ "))
                        SetSignalComment(line, dbcAsString);

                    // Extract ValueTables
                    else if (line.StartsWith("VAL_ "))
                        SetValueTable(line);
                }
            }
            catch
            {
                throw;
            }
        }

        private void AddNodes(string nodesStr)
        {
            Nodes = nodesStr.Split(new string[] { " " }, StringSplitOptions.None).Skip(1).ToList();
        }

        private bool CheckExtID(ref uint id)
        {
            // For extended ID bit 31 is always 1
            if (id >= 0x80000000)
            {
                id -= 0x80000000;
                return true;
            }
            else
                return false;
        }

        private void AddMessage(string msgStr)
        {
            Message msg = new Message();
            string[] record = msgStr.Split(new string[] { " " }, StringSplitOptions.None);

            msg.ID = uint.Parse(record[1]);
            msg.IsExtID = CheckExtID(ref msg.ID);
            msg.Name = record[2].Substring(0, record[2].Length - 1);
            msg.DLC = byte.Parse(record[3]);
            msg.Transmitter = record[4];

            Messages.Add(msg);
        }

        private void AddSignal(int idxMsg, uint msgID, string sigStr)
        {
            Signal sig = new Signal();
            int mux = 0;
            string[] records = sigStr.Split(new string[] { " ", "|", "@", "(", ")", "[", "|", "]" }, StringSplitOptions.RemoveEmptyEntries);

            if (records[2] != ":")    // signal is multiplexed
                mux = 1;

            sig.ID          = msgID;
            sig.Name        = records[1];
            sig.StartBit    = byte.Parse(records[3 + mux]);
            sig.Length      = byte.Parse(records[4 + mux]);
            sig.ByteOrder   = byte.Parse(records[5 + mux].Substring(0, 1));   // 0 = MSB (Motorola), 1 = LSB (Intel)
            if (records[5 + mux].Substring(1, 1) == "+")
                sig.IsSigned = 0;
            else
                sig.IsSigned = 1;

            sig.Factor      = double.Parse(records[6 + mux].Split(new string[] { "," }, StringSplitOptions.None)[0]);
            sig.Offset      = double.Parse(records[6 + mux].Split(new string[] { "," }, StringSplitOptions.None)[1]);
            sig.Minimum     = double.Parse(records[7 + mux]);
            sig.Maximum     = double.Parse(records[8 + mux]);
            sig.Unit        = records[9 + mux].Split(new string[] { "\"" }, StringSplitOptions.None)[1];
            sig.Receiver    = records[10 + mux].Split(new string[] { "," }, StringSplitOptions.None);  // can be multiple receivers splitted by ","

            Messages[idxMsg].Signals.Add(sig);
        }

        private void SetCycleTime(string cycleStr)
        {
            try
            {
                string[] records = cycleStr.Split(new string[] {" " , ";" }, StringSplitOptions.RemoveEmptyEntries);

                ulong msgID = ulong.Parse(records[3]);
                int idxMsg = Messages.FindIndex(x => x.ID == msgID);
                if (idxMsg >= 0)
                    Messages[idxMsg].CycleTime = int.Parse(records[4]);
            }
            catch
            {
            }
        }

        private void SetInitialValue(string initStr)
        {
            try
            {
                string[] records = initStr.Split(new string[] {" " , ";" }, StringSplitOptions.RemoveEmptyEntries);

                string sigName = records[4];
                foreach (var msg in Messages)
                {
                    int idxSig = msg.Signals.FindIndex(x => x.Name == sigName);
                    if (idxSig >= 0)
                    {
                        msg.Signals[idxSig].InitialValue = float.Parse(records[5]) * msg.Signals[idxSig].Factor + msg.Signals[idxSig].Offset;
                        break;
                    }
                }
            }
            catch
            {
            }
        }

        private void SetSignalComment(string sigCommentStr, string dbcStr)
        {
            try
            {
                string[] records = sigCommentStr.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                string sigName = records[3];
                foreach (var msg in Messages)
                {
                    int idxSig = msg.Signals.FindIndex(x => x.Name == sigName);
                    if (idxSig >= 0)
                    {

                        int idxFrom = dbcStr.IndexOf(" " + sigName + " \"") + sigName.Length + 3;
                        int length = dbcStr.IndexOf("\";", idxFrom) - idxFrom;
                        msg.Signals[idxSig].Comment = dbcStr.Substring(idxFrom, length);
                        break;
                    }
                }
            }
            catch
            {
            }
        }

        private void SetValueTable(string valTableStr)
        {
            try
            {
                string[] records = valTableStr.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                string sigName = records[2];
                foreach (var msg in Messages)
                {
                    int idxSig = msg.Signals.FindIndex(x => x.Name == sigName);
                    if (idxSig >= 0)
                    {
                        int idxFrom = valTableStr.IndexOf(records[2]) + records[2].Length + 1;
                        int length = valTableStr.LastIndexOf(";") - idxFrom;
                        msg.Signals[idxSig].ValueTable = valTableStr.Substring(idxFrom, length);
                        msg.Signals[idxSig].ValueTable = msg.Signals[idxSig].ValueTable.Replace("\" ", "\"" + Environment.NewLine);
                    }
                }
            }
            catch
            {
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
        private uint64_T MirrorMsg(uint64_T msg)
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
        private uint8_T GetStartBitLE(Signal signal)
        {
            uint8_T startByte = (uint8_T)(signal.StartBit / 8);
            return (uint8_T)(64 - (signal.Length + 8 * startByte + (8 * (startByte + 1) - (signal.StartBit + 1)) % 8));
        }

        /// <summary>
        /// Function to pack a signal into a CAN data message
        /// </summary>
        /// <param name="value">Value to be packed</param>
        /// <param name="signal">Signal containing dbc information</param>
        /// <returns>Returns a 64 bit unsigned data message</returns>
        public uint64_T TxSignalPack(double value, Signal signal)
        {
            int64_T iVal; 
            uint64_T bitMask = (1UL << signal.Length) - 1;

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
        public uint64_T TxStatePack(uint64_T value, Signal signal)
        {
            uint64_T bitMask = (1UL << signal.Length) - 1;
    
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
        public double RxSignalUnpack(uint64_T RxMsg64, Signal signal)
        {
            int64_T iVal; 
            uint64_T bitMask = (1UL << signal.Length) - 1;

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
        public uint64_T RxStateUnpack(uint64_T RxMsg64, Signal signal)
        {
            uint64_T iVal; 
            uint64_T bitMask = (1UL << signal.Length) - 1;

            // Unpack signal
            if (signal.ByteOrder != 0)  // Little endian (Intel)
                iVal = (RxMsg64 >> signal.StartBit) & bitMask;
            else                        // Big endian (Motorola)
                iVal = (MirrorMsg(RxMsg64) >> GetStartBitLE(signal)) & bitMask;

            // Apply scaling
            return iVal;
        }

    }

}


