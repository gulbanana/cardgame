using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Feast : ActionCardBase
    {
        public override int Cost => 4;

        public override string Text => @"
        <lines>
            <run>Trash this card.</run>
            <spans>
                <run>Gain a card costing up to</run>
                <sym suffix='.'>coin5</sym>
            </spans>
        </lines>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.Trash("Feast", Zone.InPlay);
            
            var gainedCard = await host.SelectCard(
                "Choose a card to gain.", 
                Zone.SupplyAvailable, 
                card => card.GetCost(host) <= 5
            );

            host.Gain(gainedCard);
        }
    }
}