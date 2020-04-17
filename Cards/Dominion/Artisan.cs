using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Artisan : ActionCardBase
    {
        public override string Art => "dom-artisan";
        public override int Cost => 6;

        public override string Text => @"<paras>
            <spans>
                <run>Gain a card to your hand costing up to</run>
                <sym suffix='.'>coin5</sym>
            </spans>
            <run>Put a card from your hand onto your deck.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            var gainedCard = await host.SelectCard(
                "Choose a card to gain.", 
                Zone.Supply, 
                cards => cards.Where(card => card.GetCost(host.GetModifiers()) <= 5)
            );
            host.Gain(gainedCard.Name, Zone.Hand);

            var placedCard = await host.SelectCard(
                "Choose a card to put onto your deck.", 
                Zone.Hand
            );
            host.PlaceOnDeck(placedCard, Zone.Hand);            
        }
    }
}