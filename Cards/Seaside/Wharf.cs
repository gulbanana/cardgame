using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class Wharf : DurationCardBase
    {
        public override int Cost => 5;

        public override string Text => @"<lines>
            <run>Now and at the start of your next turn:</run>
            <spans>
                <bold>+2 Cards</bold>
                <run>and</run>
                <bold>+1 Buy.</bold>            
            </spans>
        </lines>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(2);
            host.AddBuys(1);
        }

        protected override void OnBeginTurn(IActionHost host)
        {
            host.DrawCards(2);
            host.AddBuys(1);
        }
    }
}