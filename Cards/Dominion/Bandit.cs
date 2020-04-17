using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Bandit : ActionCardBase
    {
        public override string SubType => "Attack";
        public override string Art => "dom-bandit";
        public override int Cost => 5;

        public override string Text => @"<run>
            Gain a Gold. Each other player reveals the top 2 cards of their deck, trashes a revealed Treasure other than a copper, and discards the rest.
        </run>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.Gain("Gold");
            var trashed = new List<ICard>();
            
            await host.Attack(async player => 
            {
                var top2 = player.RevealAll(Zone.DeckTop2);
                var trashed = false;
                foreach (var card in top2)
                {
                    if (!trashed && card.Type == CardType.Treasure && card.Name != "Copper")
                    {
                        trashed = true;
                        player.Trash(card, Zone.DeckTop2);
                    }
                    else
                    {
                        player.Discard(card, Zone.DeckTop2);
                    }
                }
            });
        }
    }
}