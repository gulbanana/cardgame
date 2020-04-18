using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Upgrade : ActionCardBase
    {
        public override string Art => "int-upgrade";
        public override int Cost => 5;

        public override string Text => @"<paras>
            <block>
                <lines>
                    <run>+1 Card</run>
                    <run>+1 Action</run>
                </lines>
            </block>
            <spans>
                <run>Trash a card from your hand. Gain a card costing exactly</run>
                <sym>coin1</sym>
                <run>more than it.</run>
            </spans>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(1);

            var trashed = await host.SelectCard("Choose a card to trash.", Zone.Hand);
            if (trashed != null)
            {
                host.Trash(trashed);

                var value = trashed.GetCost(host.GetModifiers()) + 1;
                var gained = await host.SelectCard("Choose a card to gain.", cards => cards.Where(c => c.GetCost(host.GetModifiers()) == value));
                if (gained != null)
                {
                    host.Gain(gained);
                }
            }
        }
    }
}