using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class Salvager : ActionCardBase
    {
        public override int Cost => 3;

        public override string Text => @"<paras>
            <bold>+1 Buy</bold>
            <spans>
                <run>Trash a card from your hand.</run>
                <sym prefix='+'>coin1</sym>
                <run>per</run>
                <sym>coin1</sym>
                <run>it costs.</run>
            </spans>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.AddBuys(1);

            var trashed = await host.SelectCard("Choose a card to trash.");
            if (trashed != null)
            {
                host.Trash(trashed);
                host.AddCoins(trashed.GetCost(host));
            }
        }
    }
}