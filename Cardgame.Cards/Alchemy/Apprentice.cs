using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Alchemy
{
    public class Apprentice : ActionCardBase
    {
        public override Cost Cost => 5;

        public override string Text => @"<paras>
            <bold>+1 Action</bold>
            <lines>
                <run>Trash a card from your hand.</run>
                <spans>
                    <bold>+1 Card</bold>
                    <run>per</run>
                    <sym>coin1</sym>
                    <run>it costs.</run>
                </spans>
                <spans>
                    <bold>+2 Cards</bold>
                    <run>if it has</run>
                    <sym>potion</sym>
                    <run>in its cost.</run>
                </spans>
            </lines>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.AddActions(1);

            var trashed = await host.SelectCard("Choose a card to trash.");
            if (trashed != null)
            {
                var cost = trashed.GetCost(host);
                host.Trash(trashed);

                host.DrawCards(cost.Coins + (cost.Potion ? 2 : 0));
            }
        }
    }
}