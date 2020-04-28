using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Feast : ActionCardBase
    {
        public override Cost Cost => 4;

        public override string Text => @"
        <lines>
            <run>Trash this card.</run>
            <spans>
                <run>Gain a card costing up to</run>
                <sym>coin5.</sym>
            </spans>
        </lines>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.Trash("Feast", Zone.InPlay);
            
            var gainedCard = await host.SelectCard(
                "Choose a card to gain.", 
                Zone.SupplyAvailable, 
                card => card.GetCost(host).LessThanOrEqual(5)
            );

            await host.Gain(gainedCard);
        }
    }
}