using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cardgame.Cards.Dominion
{
    public class Thief : ActionCardModel
    {
        public override string SubType => "Attack";
        public override string Art => "dom-thief";
        public override int Cost => 4;

        public override string Text => @"<small>
            <lines>
                <run>Each other player reveals the top 2 cards of their deck.</run>
                <run>If they revealed any Treasure cards, they trash one of them that you choose. You may gain any or all of these trashed cards. They discard the other revealed cards.</run>
            </lines>
        </small>";

        protected override async Task ActAsync(IActionHost host)
        {
            var trashed = new List<CardModel>();
            
            await host.Attack(async player => 
            {
                var top2 = player.RevealAll(Zone.DeckTop2);
                var treasures = top2.OfType<TreasureCardModel>();
                if (treasures.Any())
                {
                    if (treasures.Count() == 1 || treasures.First().Equals(treasures.Last()))
                    {
                        var soleTreasure = treasures.First();                        
                        player.Trash(soleTreasure, Zone.DeckTop2);
                        trashed.Add(soleTreasure);
                    }
                    else
                    {
                        var chosenTreasure = await host.SelectCard("Choose a Treasure to trash.", treasures);
                        player.Trash(chosenTreasure, Zone.DeckTop2);
                        trashed.Add(chosenTreasure);
                    }
                }
            });

            if (trashed.Any())
            {
                var gained = await host.SelectCards("Choose trashed Treasures to gain.", trashed);
                if (gained.Any())
                {
                    host.GainFrom(gained, Zone.Trash);
                }
            }
        }
    }
}