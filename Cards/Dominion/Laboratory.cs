using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Laboratory : ActionCardBase
    {
        public override string Art => "dom-laboratory";
        public override int Cost => 5;
        
        public override string Text => @"<lines>
            <bold>+2 Cards</bold>
            <bold>+1 Action</bold>
        </lines>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(2);
            host.AddActions(1);
        }
    }
}