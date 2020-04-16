using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Workshop : ActionCardModel
    {
        public override string Art => "dom-workshop";
        public override int Cost => 3;
        
        public override string Text => @"
        <spans>
            <run>Gain a card costing up to</run>
            <sym suffix='.'>coin4</sym>
        </spans>";

        protected override async Task ActAsync(IActionHost host)
        {
            var gainedCard = await host.SelectCard(
                "Choose a card to gain.", 
                Zone.Supply, 
                cards => cards.Where(card => card.Cost <= 4)
            );
            host.Gain(gainedCard.Name);
        }
    }
}