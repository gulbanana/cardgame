using System;

namespace Cardgame.Engine.Logging
{
    internal class Record : Subrecord
    {
        private readonly Action<Record> updateAny;
        public readonly int Index;
        public readonly string Header;

        public Record(int index, string header, Action<Record> updateRecord) : base(null)
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