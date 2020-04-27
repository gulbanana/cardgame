using System.Collections.Generic;
using System.Linq;

namespace Cardgame.Engine.Logging
{
    internal class Chunk
    {
        public readonly string Actor;
        public readonly List<string> TextLines;
        public int AddedCards;
        public int AddedActions;
        public int AddedBuys;
        public int AddedCoins;
        public int AddedPotions;

        public Chunk(string actor)
        {
            Actor = actor;
            TextLines = new List<string>();
        }

        public bool HasContent()
        {
            return TextLines.Any();
        }
    }
}