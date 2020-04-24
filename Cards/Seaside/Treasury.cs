using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class Treasury : ActionCardBase, IReactor
    {
        public override int Cost => 5;

        public override string Text => @"<small>
            <split compact='true'>
                <bold>
                    <lines>
                        <run>+1 Card</run>
                        <run>+1 Actions</run>
                        <sym prefix='+'>coin1</sym>
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

        public async Task<Reaction> ExecuteReactionAsync(IActionHost host, Zone reactFrom, Trigger triggerType, string triggerParameter)
        {
            if (reactFrom == Zone.This && triggerType == Trigger.DiscardCard)
            {
                var bought = host.Examine(Zone.RecentBuys);
                var boughtVictories = bought.Any(card => card.Types.Contains(CardType.Victory));
                if (!boughtVictories && await host.YesNo("Treasury", "<run>Put</run><card>Treasury</card><run>back onto your deck?</run>"))
                {
                    return Reaction.After(() =>
                    {
                        host.PutOnDeck("Treasury", Zone.Discard);
                    });
                }
            }

            return Reaction.None();
        }
    }
}