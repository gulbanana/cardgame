using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class TreasureMap : ActionCardBase
    {
        public override Cost Cost => 4;

        public override string Text => "Trash this and a Treasure Map from your hand. If you trashed two Treasure Maps, gain 4 Golds onto your deck.";

        protected override async Task ActAsync(IActionHost host)
        {
            host.Trash("TreasureMap", Zone.InPlay);
            var hand = host.Examine(Zone.Hand);
            if (hand.Any(card => card.Name == "TreasureMap"))
            {
                host.Trash("TreasureMap", Zone.Hand);
                await host.Gain(new[]{"Gold", "Gold", "Gold", "Gold"}, Zone.Deck);
            }
        }
    }
}