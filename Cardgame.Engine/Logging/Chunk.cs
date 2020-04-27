using System.Collections.Generic;
using System.Linq;

namespace Cardgame.Engine.Logging
{
    internal class Chunk
    {
        public readonly string Actor;

        public bool Reshuffled { get; set; }
        public List<string> AddedCards { get; set; }
        public int AddedActions { get; set; }
        public int AddedBuys { get; set; }
        public int AddedCoins { get; set; }
        public int AddedPotions { get; set; }

        public List<Movement> Movements { get; }

        public List<string> Lines { get; }

        public Chunk(string actor)
        {
            Actor = actor;
            AddedCards = new List<string>();
            Movements = new List<Movement>();
            Lines = new List<string>();
        }

        public bool HasVanillaContent()
        {
            return Reshuffled || AddedCards.Count > 0 || AddedActions > 0 || AddedBuys > 0 || AddedCoins > 0 || AddedPotions > 0;
        }

        public bool HasContent()
        {
            return HasVanillaContent() || Movements.Any() || Lines.Any();
        }
    }
}