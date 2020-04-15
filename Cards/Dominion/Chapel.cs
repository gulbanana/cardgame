using System.Linq;
using System.Threading.Tasks;

namespace Cardgame.Cards.Dominion
{
    public class Chapel : ActionCardModel
    {
        public override string Art => "dom-chapel";
        public override int Cost => 2;
        
        public override string Text => @"<run>
            Trash up to 4 cards from your hand.
        </run>";

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