using System.Linq;
using System.Threading.Tasks;

namespace Cardgame.Cards
{
    public class Mine : ActionCardModel
    {
        public override string Art => "dom-mine";
        public override int Cost => 5;

        public override string Text => @"
        <lines>
            <run>Trash a Treasure card from your hand.</run>
            <spans>
                <run>Gain a Treasure card costing up to</run>
                <sym>coin3</sym>
                <run>more; put it into your hand.</run>
            </spans>
        </lines>";

        protected override async Task ActAsync(IActionHost host)
        {
            var trashedCard = await host.SelectCard(
                "Choose a Treasure to trash.", 
                cards => cards.OfType<TreasureCardModel>()
            );
            host.TrashCard(trashedCard.Name);

            var gainedCard = await host.SelectCard(
                "Choose a Treasure to gain.", 
                CardSource.Kingdom, 
                cards => cards.OfType<TreasureCardModel>().Where(card => card.Cost <= trashedCard.Cost + 3)
            );
            host.GainCardToHand(gainedCard.Name);
        }
    }
}