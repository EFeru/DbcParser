using System.Collections.Generic;

namespace DbcParserLib.Model
{
    public class Node
    {
        public string Name;
        public string Comment;
    }

    public class Message
    {
        public uint ID;
        public bool IsExtID;
        public string Name;
        public byte DLC;
        public string Transmitter;
        public string Comment;
        public int CycleTime;
        public List<Signal> Signals = new List<Signal>();
    }

    public class Signal
    {
        public uint ID;
        public string Name;
        public ushort StartBit;
        public byte Length;
        public byte ByteOrder = 1;
        public byte IsSigned;
        public double InitialValue;
        public double Factor = 1;
        public bool IsInteger = false;
        public double Offset;
        public double Minimum;
        public double Maximum;
        public string Unit;
        public string[] Receiver;
        public string ValueTable;
        public string Comment;
        public string Multiplexing;
    }
}