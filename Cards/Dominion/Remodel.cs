using System.Linq;
using System.Threading.Tasks;

namespace Cardgame.Cards.Dominion
{
    public class Remodel : ActionCardModel
    {
        public override string Art => "dom-remodel";
        public override int Cost => 4;
        
        public override string Text => @"
        <lines>
            <run>Trash a card from your hand.</run>
            <spans>
                <run>Gain a card costing up to</run>
                <sym>coin2</sym>
                <run>more than the trashed card.</run>
            </spans>
        </lines>";

        protected override async Task ActAsync(IActionHost host)
        {
            var trashedCard = await host.SelectCard("Choose a card to trash.");
            host.Trash(trashedCard.Name);

            var gainedCard = await host.SelectCard(
                "Choose a card to gain.", 
                CardSource.Kingdom, 
                cards => cards.Where(card => card.Cost <= trashedCard.Cost + 2)
            );
            host.Gain(gainedCard.Name);
        }
    }
}