using System;
using System.Linq;
using Cardgame.API;

namespace Cardgame.All
{
    public static class Supply
    {        
        public static int GetInitialCount(int nPlayers, string card)
        {
            var victoryCount = nPlayers == 2 ? 8 : 12;
            return card switch
            {
                "Copper" => 60 - (nPlayers * 7),
                "Silver" => 40,
                "Gold" => 30,
                "Curse" => (nPlayers - 1) * 10,
                "Potion" => 16,
                string id => All.Cards.ByName(id).Types.Contains(CardType.Victory) ? victoryCount : 10
            };
        }  
    }
}