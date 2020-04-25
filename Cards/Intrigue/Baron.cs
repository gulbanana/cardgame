using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Baron : ActionCardBase
    {
        public override string Art => "int-baron";
        public override Cost Cost => 4;

        public override string Text => @"<paras>
            <bold>+1 Buy</bold>
            <spans>
                <run>You may discard an Estate for</run>
                <sym prefix='+' suffix='.'>coin4</sym>
                <run>If you don't, gain an Estate.</run>
            </spans>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.AddBuys(1);

            var hand = host.Examine(Zone.Hand);
            if (hand.Any(card => card.Name == "Estate"))
            {
                if (await host.YesNo("Baron", @"<run>Discard an</run>
                                                <card>Estate</card>
                                                <run>for</run>
                                                <sym prefix='+' suffix='?'>coin4</sym>"))
                {
                    host.Discard("Estate");
                    host.AddCoins(4);
                    return;
                }
            }

            host.Gain("Estate");
        }
    }
}