using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cardgame.API;
using Cardgame.Cards.Base;
using Cardgame.Shared;

namespace Cardgame.All
{
    public class Score
    {    
        public static Score Calculate(GameModel game, string player)
        {            
            var dominion = game.IsStarted ? 
                game.Decks[player]
                    .Concat(game.Hands[player])
                    .Concat(game.Discards[player])
                    .Concat(game.PlayedCards[player])
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
                var curseCards = dominion.Select(All.Cards.ByName).OfType<Curse>();
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

        public string Player { get; private set; }
        public int Total { get; private set; }
        public (string card, int count, int subtotal)[] Subtotals { get; private set; }        

        public string Text()
        {
            var builder = new StringBuilder();
            builder.AppendLine("<lines>");
                builder.AppendLine($"<spans><player>{Player}</player><run>scored:</run></spans>");                    
                foreach (var group in Subtotals)
                {
                    builder.AppendLine("<spans>");
                    builder.AppendLine($"<card>{group.card}</card>");
                    builder.AppendLine($"<run>x{group.count}: {group.subtotal} VP</run>");
                    builder.AppendLine("</spans>");
                }                
                builder.AppendLine($"<run>Total: {Total} Victory Points</run>");
            builder.AppendLine("</lines>");
            return builder.ToString();
        }
    }
}