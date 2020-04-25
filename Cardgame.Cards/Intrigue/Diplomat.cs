using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Diplomat : AttackReactionCardBase
    {
        public override string Art => "int-diplomat";
        public override Cost Cost => 4;        

        public override string Text => @"<split compact='true'>
            <lines>
                <bold>+2 Cards</bold>
                <small>
                    <run>If you have 5 or fewer cards in hand (after drawing),</run>
                    <bold>+2 Actions.</bold>
                </small>
            </lines>
            <small>When another player plays an Attack card, you may first reveal this from a hand of 5 or more cards, to draw 2 cards then discard 3.</small>
        </split>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(2);
            if (host.Count(Zone.Hand) <= 5)
            {
                host.AddActions(2);
            }
        }

        protected override async Task BeforeAttackAsync(IActionHost host)
        {
            host.DrawCards(2);
            var discarded = await host.SelectCards("Choose cards to discard.", Zone.Hand, 3, 3);
            host.Discard(discarded);
        }
    }
}