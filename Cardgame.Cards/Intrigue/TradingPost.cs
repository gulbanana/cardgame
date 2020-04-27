using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class TradingPost : ActionCardBase
    {
        public override string Art => "int-trading-post";
        public override Cost Cost => 5;

        public override string Text => @"Trash 2 cards from your hand. If you did, gain a silver to your hand.";

        protected override async Task ActAsync(IActionHost host)
        {
            var handSize = host.Count(Zone.Hand);
            var trashed = handSize <= 2 ? host.Examine(Zone.Hand) : await host.SelectCards("Choose cards to trash.", Zone.Hand, 2, 2);

            host.Trash(trashed);

            if (trashed.Count() == 2)
            {
                await host.Gain("Silver", Zone.Hand);
            }
        }
    }
}