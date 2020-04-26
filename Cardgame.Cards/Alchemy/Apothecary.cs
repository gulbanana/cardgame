using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Alchemy
{
    public class Apothecary : ActionCardBase
    {
        public override Cost Cost => new Cost(2, true);

        public override string Text => @"<paras>
            <lines>
                <bold>+1 Card</bold>
                <bold>+1 Action</bold>
            </lines>
            <run>Reveal the top 4 cards of your deck. Put the Coppers and Potions into your hand. Put the rest back in any order.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(1);

            var top4 = host.Reveal(Zone.DeckTop(4));
            var drawn = top4.Where(card => card.Name == "Copper" || card.Name == "Potion").ToArray();
            var notDrawn = top4.Without(drawn).ToArray();

            host.PutIntoHand(drawn, Zone.Deck);

            if (notDrawn.Length > 1)
            {
                var reordered = await host.OrderCards("Put these cards back in any order.", notDrawn);
                host.Reorder(reordered, Zone.DeckTop(notDrawn.Length));
            }
        }
    }
}