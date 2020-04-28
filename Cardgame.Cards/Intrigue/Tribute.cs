using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Tribute : ActionCardBase
    {
        public override Cost Cost => 5;

        public override string Text => @"<small>
            <paras>
                <lines>
                    <run>The player to your left reveals then discards the top 2 cards of their deck.</run>
                    <run>For each differently named card revealed, if it is an...</run>
                </lines>
                <lines>
                    <spans>
                        <run>Action card,</run>
                        <bold>+2 Actions</bold>
                    </spans>
                    <spans>
                        <run>Treasure card,</run>
                        <bold><sym>+coin2</sym></bold>
                    </spans>
                    <spans>
                        <run>Victory card,</run>
                        <bold>+2 Cards</bold>
                    </spans>
                </lines>
            </paras>
        </small>";

        protected override async Task ActAsync(IActionHost host)
        {
            await host.OnePlayer(host.GetPlayerToLeft(), target =>
            {
                foreach (var card in target.Examine(Zone.DeckTop(2)))
                {
                    target.Discard(card, Zone.Deck);
                    if (card.Types.Contains(CardType.Action)) host.AddActions(2);
                    if (card.Types.Contains(CardType.Treasure)) host.AddCoins(2);
                    if (card.Types.Contains(CardType.Victory)) host.DrawCards(2);
                }
            });
        }
    }
}