using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class Lighthouse : DurationCardBase
    {
        public override Cost Cost => 2;

        public override string Text => @"<split compact='true'>
            <lines>
                <bold>+1 Action</bold>
                <spans>
                    <run>Now and at the start of your next turn:</run>
                    <sym prefix='+' suffix='.'>coin1</sym>
                </spans>
            </lines>
            <run>
                While this is in play, when another player plays an Attack card, it doesn't affect you.
            </run>
        </split>";

        protected override void Act(IActionHost host)
        {
            host.AddActions(1);
            host.AddCoins(1);
        }

        protected override void OnBeginTurn(IActionHost host)
        {
            host.AddCoins(1);
        }

        protected override void OnBeforePlayCard(IActionHost host, ICard card)
        {
            host.PreventAttack(true);
        }

        protected override void OnAfterPlayCard(IActionHost host, ICard card)
        {
            host.PreventAttack(false);
        }
    }
}