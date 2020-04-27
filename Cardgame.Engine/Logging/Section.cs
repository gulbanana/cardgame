using System.Collections.Generic;

namespace Cardgame.Engine.Logging
{
    internal class Section
    {
        public readonly Chunk Chunk;
        public readonly Subrecord Subrecord;

        public Section(string player)
        {
            Chunk = new Chunk(player);
        }

        public Section(Subrecord subrecord)
        {
            Subrecord = subrecord;
        }
    }
}