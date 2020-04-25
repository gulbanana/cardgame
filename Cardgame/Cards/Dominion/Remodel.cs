using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Remodel : ActionCardBase
    {
        public override string Art => "dom-remodel";
        public override Cost Cost => 4;
        
        public override string Text => @"<lines>
            <run>Trash a card from your hand.</run>
            <spans>
                <run>Gain a card costing up to</run>
                <sym>coin2</sym>
                <run>more than it.</run>
            </spans>
        </lines>";

        protected override async Task ActAsync(IActionHost host)
        {
            var modifiers = host.GetModifiers();

            var trashedCard = await host.SelectCard("Choose a card to trash.");
            host.Trash(trashedCard.Name);

            var gainedCard = await host.SelectCard(
                "Choose a card to gain.", 
                Zone.SupplyAvailable, 
                card => card.GetCost(modifiers).LessThanOrEqual(trashedCard.GetCost(modifiers).Plus(2))
            );
            host.Gain(gainedCard.Name);
        }
    }
}