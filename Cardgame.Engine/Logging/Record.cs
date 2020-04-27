using System;

namespace Cardgame.Engine.Logging
{
    internal class Record : Subrecord
    {
        private readonly Action<Record> updateAny;
        public readonly int Index;
        public Event Event { get; }

        public Record(int index, string actor, Event @event, Action<Record> updateRecord) : base(actor)
        {
            this.updateAny = updateRecord;
            Index = index;
            Event = @event;
        }

        public override void Update()
        {
            updateAny(this);
        }
    }
}