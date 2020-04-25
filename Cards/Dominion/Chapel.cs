using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Chapel : ActionCardBase
    {
        public override string Art => "dom-chapel";
        public override Cost Cost => 2;
        
        public override string Text => @"Trash up to 4 cards from your hand.";

        protected override async Task ActAsync(IActionHost host)
        {
            var cards = await host.SelectCards("Choose cards to trash.", 0, 4);
            if (cards.Any())
            {
                host.Trash(cards);            
            }
        }
    }
}