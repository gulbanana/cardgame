using System.Collections.Generic;

namespace Cardgame.Engine.Logging
{
    public class Section
    {
        public readonly List<string> Chunk;
        public readonly Subrecord Subrecord;

        public Section()
        {
            Chunk = new List<string>();
        }

        public Section(Subrecord subrecord)
        {
            Subrecord = subrecord;
        }
    }
}