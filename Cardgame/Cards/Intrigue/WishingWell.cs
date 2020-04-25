using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class WishingWell : ActionCardBase
    {
        public override string Art => "int-wishing-well";
        public override Cost Cost => 3;

        public override string Text => @"<paras>
            <lines>
                <bold>+1 Card</bold>
                <bold>+1 Action</bold>
            </lines>
            <run>Name a card, then reveal the top card of your deck. If you named it, put it into your hand.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(1);

            var named = await host.SelectCard("Name a card.", Zone.SupplyAll);
            host.Name(named);

            var top1 = host.Reveal(Zone.DeckTop(1));
            if (named.Equals(top1.SingleOrDefault()))
            {
                host.PutIntoHand(named, Zone.Deck);
            }
        }
    }
}