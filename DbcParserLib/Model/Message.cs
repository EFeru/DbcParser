using System;
using System.Collections.Generic;
using System.Text;

namespace DbcParserLib.Model
{
    public class Message
    {
        public uint ID { get; }
        public bool IsExtID { get; }
        public string Name { get; }
        public ushort DLC { get; }
        public string Transmitter { get; }
        public string Comment { get; }
        public int CycleTime { get; }
        public IReadOnlyList<Signal> Signals;
        public IReadOnlyDictionary<string, CustomProperty> CustomProperties;

        internal Message(EditableMessage message, IReadOnlyList<Signal> signals)
        {
            ID = message.ID;
            IsExtID = message.IsExtID;
            Name = message.Name;
            DLC = message.DLC;
            Transmitter = message.Transmitter;
            Comment = message.Comment;
            CycleTime= message.CycleTime;
            Signals = signals;
            CustomProperties = message.CustomProperties;
        }
    }

    internal class EditableMessage
    {
        public uint ID;
        public bool IsExtID;
        public string Name;
        public ushort DLC;
        public string Transmitter;
        public string Comment;
        public int CycleTime;
        public List<EditableSignal> Signals = new List<EditableSignal>();
        public Dictionary<string, CustomProperty> CustomProperties = new Dictionary<string, CustomProperty>();

        public Message CreateMessage()
        {
            var signals = new List<Signal>();
            foreach(var signal in Signals)
            {
                signals.Add(signal.CreateSignal());
            }
            return new Message(this, signals);
        }
    }
}
