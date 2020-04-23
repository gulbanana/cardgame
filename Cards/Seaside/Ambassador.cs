using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class Ambassador : AttackCardBase
    {
        public override int Cost => 3;

        public override string Text => @"<run>
             Reveal a card from your hand. Return up to 2 copies of it from your hand to the Supply. Then each other player gains a copy of it.
        </run>";

        protected override async Task ActAsync(IActionHost host)
        {
            var revealed = await host.SelectCard("Choose a card to reveal.");
            if (revealed != null)
            {
                host.Reveal(revealed);
                var hand = host.Examine(Zone.Hand);
                var copies = hand.Where(c => c == revealed);

                var returned = await host.SelectCards("Choose copies to return.", copies);
                host.ReturnToSupply(returned);

                await host.Attack(target => target.Gain(revealed));
            }
        }
    }
}