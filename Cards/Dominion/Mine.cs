using System.Linq;
using System.Threading.Tasks;

namespace Cardgame.Cards.Dominion
{
    public class Mine : ActionCardModel
    {
        public override string Art => "dom-mine";
        public override int Cost => 5;

        public override string Text => @"
        <spans>
            <run>You may trash a Treasure from your hand.</run>
            <spans>
                <run>Gain a Treasure to your hand costing up to</run>
                <sym>coin3</sym>
                <run>more than it.</run>
            </spans>
        </spans>";

        protected override async Task ActAsync(IActionHost host)
        {
            var trashedCard = await host.SelectCard(
                "Choose a Treasure to trash.", 
                cards => cards.OfType<TreasureCardModel>()
            );

            if (trashedCard != null)
            {
                host.Trash(trashedCard);
            
                var gainedCard = await host.SelectCard(
                    "Choose a Treasure to gain.", 
                    Zone.Supply, 
                    cards => cards.OfType<TreasureCardModel>().Where(card => card.Cost <= trashedCard.Cost + 3)
                );

                host.Gain(gainedCard.Name, Zone.Hand);
            }
        }
    }
}