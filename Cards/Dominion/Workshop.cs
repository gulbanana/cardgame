using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Workshop : ActionCardBase
    {
        public override string Art => "dom-workshop";
        public override Cost Cost => 3;
        
        public override string Text => @"
            <run>Gain a card costing up to</run>
            <sym suffix='.'>coin4</sym>
        ";

        protected override async Task ActAsync(IActionHost host)
        {
            var gainedCard = await host.SelectCard(
                "Choose a card to gain.", 
                Zone.SupplyAvailable, 
                card => card.GetCost(host).LessThanOrEqual(4)
            );
            host.Gain(gainedCard.Name);
        }
    }
}