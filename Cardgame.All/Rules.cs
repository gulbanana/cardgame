using System.Collections.Generic;
using System.Linq;
using Cardgame.API;
using Cardgame.Model;

namespace Cardgame.All
{
    // game formulas used on both client and server
    public static class Rules
    {
        public static Score CalculateScore(GameModel game, string player)
        {
            var dominion = game.IsStarted ? 
                game.Decks[player]
                    .Concat(game.Hands[player])
                    .Concat(game.Discards[player])
                    .Concat(game.PlayedCards[player])
                    .Concat(game.PlayedCards[player].Where(card => game.Attachments.ContainsKey(card)).Select(card => game.Attachments[card]))
                    .Concat(game.PlayerMatCards[player].Values.SelectMany(card => card))
                    .Names()
                    .ToArray() : 
                new[] { "Estate", "Estate", "Estate" } ;

            var victoryCards = dominion.Select(All.Cards.ByName).OfType<IVictoryCard>().GroupBy(card => card.Name);

            var total = 0;
            var subtotals = new List<(string, int, int)>();

                foreach (var group in victoryCards)
                {
                    var exemplar = group.First();
                    var score = exemplar.Score(dominion) * group.Count();
                    total += score;
                    subtotals.Add((group.Key, group.Count(), score));
                }
                var curseCards = dominion.Select(All.Cards.ByName).Where(card => card.Types.Contains(CardType.Curse));
                if (curseCards.Any())
                {
                    var score = curseCards.Count();
                    total -= score;
                    subtotals.Add(("Curse", score, -score));
                }

            return new Score 
            {
                Player = player,
                Total = total,
                Subtotals = subtotals.ToArray()
            };
        }

        public static int GetInitialSupply(int nPlayers, string card)
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

        public static Cost MaxAffordableCost(GameModel model)
        {
            return new Cost(model.CoinsRemaining, model.PotionsRemaining > 0);
        }
    }
}