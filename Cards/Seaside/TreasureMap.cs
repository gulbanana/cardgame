using System.Linq;
using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class TreasureMap : ActionCardBase
    {
        public override int Cost => 4;

        public override string Text => "Trash this and a Treasure Map from your hand. If you trashed two Treasure Maps, gain 4 Golds onto your deck.";

        protected override void Act(IActionHost host)
        {
            host.Trash("TreasureMap", Zone.InPlay);
            var hand = host.Examine(Zone.Hand);
            if (hand.Any(card => card.Name == "TreasureMap"))
            {
                host.Trash("TreasureMap", Zone.Hand);
                host.Gain("Gold", Zone.Deck);
                host.Gain("Gold", Zone.Deck);
                host.Gain("Gold", Zone.Deck);
                host.Gain("Gold", Zone.Deck);
            }
        }
    }
}