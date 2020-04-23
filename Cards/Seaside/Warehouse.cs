using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class Warehouse : ActionCardBase
    {
        public override int Cost => 3;

        public override string Text => @"<paras>
            <lines>
                <bold>+3 Cards</bold>
                <bold>+1 Action</bold>
            </lines>
            <run>Discard 3 cards.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(3);
            host.AddActions(1);

            var discarded = await host.SelectCards("Choose cards to discard.", 3, 3);
            host.Discard(discarded);
        }
    }
}