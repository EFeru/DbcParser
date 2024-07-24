using System.Collections.Generic;

namespace DbcParserLib.Model
{
    internal class ImmutableMessage
    {
        public uint ID { get; }
        public bool IsExtID { get; }
        public string Name { get; }
        public ushort DLC { get; }
        public string Transmitter { get; }
        public string Comment { get; }
        public int CycleTime { get; }
        public IReadOnlyList<ImmutableSignal> Signals { get; }
        public IReadOnlyDictionary<string, CustomProperty> CustomProperties { get; }

        internal ImmutableMessage(Message message, IReadOnlyList<ImmutableSignal> signals)
        {
            message.CycleTime(out var cycleTime);

            ID = message.ID;
            IsExtID = message.IsExtID;
            Name = message.Name;
            DLC = message.DLC;
            Transmitter = message.Transmitter;
            Comment = message.Comment;
            CycleTime = cycleTime;
            Signals = signals;

            //TODO: remove explicit cast (CustomProperty in Message class should be Dictionary instead IDictionary)
            CustomProperties = (IReadOnlyDictionary<string, CustomProperty>)message.CustomProperties;
        }
    }

    public class Message
    {
        public uint ID;
        public bool IsExtID;
        public string Name;
        public ushort DLC;
        public string Transmitter;
        public string Comment;
        
        public List<Signal> Signals = new List<Signal>();
        public IDictionary<string, CustomProperty> CustomProperties = new Dictionary<string, CustomProperty>();

        internal ImmutableMessage CreateMessage()
        {
            var signals = new List<ImmutableSignal>();
            foreach(var signal in Signals)
            {
                signals.Add(signal.CreateSignal());
            }
            return new ImmutableMessage(this, signals);
        }
    }
}
