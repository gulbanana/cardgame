using System.Linq;
using System.Threading.Tasks;

namespace Cardgame.Cards.Dominion
{
    public class Feast : ActionCardModel
    {
        public override string Art => "dom-feast";
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
                Zone.Supply, 
                cards => cards.Where(card => card.Cost <= 5)
            );

            host.Gain(gainedCard);
        }
    }
}