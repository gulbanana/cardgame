using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class Tactician : DurationCardBase
    {
        public override int Cost => 5;

        public override string Text => @"<spans>
            <run> If you have at least one card in hand, discard your hand, and at the start of your next turn,</run>
            <bold>+5 Cards, +1 Action,</bold>
            <run>and</run>
            <bold>+1 Buy.</bold>          
        </spans>";

        protected override void Act(IActionHost host)
        {
            if (host.Count(Zone.Hand) > 0)
            {
                host.Discard(Zone.Hand);
            }
            else
            {
                host.CompleteDuration();
            }
        }

        protected override void OnBeginTurn(IActionHost host)
        {
            host.DrawCards(5);
            host.AddActions(1);
            host.AddBuys(1);
        }
    }
}