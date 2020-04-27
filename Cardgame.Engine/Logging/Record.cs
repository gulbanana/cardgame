using System;

namespace Cardgame.Engine.Logging
{
    internal class Record : Subrecord
    {
        private readonly Action<Record> updateAny;
        public readonly int Index;
        public string Header { get; }

        public Record(int index, string actor, string header, Action<Record> updateRecord) : base(actor)
        {
            this.updateAny = updateRecord;
            Index = index;
            Header = header;
        }

        public override void Update()
        {
            updateAny(this);
        }
    }
}