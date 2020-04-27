using System.Collections.Generic;
using System.Linq;

namespace Cardgame.Engine.Logging
{
    internal class Chunk
    {
        public readonly string Actor;

        public int AddedCards;
        public int AddedActions;
        public int AddedBuys;
        public int AddedCoins;
        public int AddedPotions;

        public readonly List<Movement> Movements;

        public readonly List<string> Lines;

        public Chunk(string actor)
        {
            Actor = actor;
            Movements = new List<Movement>();
            Lines = new List<string>();
        }

        public bool HasVanillaContent()
        {
            return AddedCards > 0 || AddedActions > 0 || AddedBuys > 0 || AddedCoins > 0 || AddedPotions > 0;
        }

        public bool HasContent()
        {
            return HasVanillaContent() || Movements.Any() || Lines.Any();
        }
    }
}