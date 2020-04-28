using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class Treasury : TriggeredActionCardBase
    {
        public override Cost Cost => 5;

        public override string Text => @"<small>
            <split compact='true'>
                <bold>
                    <lines>
                        <run>+1 Card</run>
                        <run>+1 Actions</run>
                        <sym>+coin1</sym>
                    </lines>
                </bold>
                <run>
                    When you discard this from play, if you didn't buy a Victory card this turn, you may put this onto your deck.
                </run>
            </split>
        </small>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(1);
            host.AddCoins(1);
        }

        protected override async Task OnDiscardFromPlayAsync(IActionHost host)
        {
            var bought = host.Examine(Zone.RecentBuys);
            var boughtVictories = bought.Any(card => card.Types.Contains(CardType.Victory));
            if (!boughtVictories && await host.YesNo("Treasury", "<run>Put</run><card>Treasury</card><run>back onto your deck?</run>"))
            {
                host.PutOnDeck("Treasury", Zone.Discard);
            }
        }
    }
}