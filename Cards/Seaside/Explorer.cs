using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class Explorer : ActionCardBase
    {
        public override Cost Cost => 5;

        public override string Text => @"You may reveal a Province from your hand. If you do, gain a Gold to your hand. If you don't, gain a Silver to your hand.";

        protected override async Task ActAsync(IActionHost host)
        {
            var hand = host.Examine(Zone.Hand);
            if (hand.Names().Contains("Province") && await host.YesNo("Explorer", "<run>Reveal a</run><card>Province</card><run>from your hand?</run>"))
            {
                host.Reveal("Province");
                host.Gain("Gold", Zone.Hand);
            }
            else
            {
                host.Gain("Silver", Zone.Hand);
            }
        }
    }
}