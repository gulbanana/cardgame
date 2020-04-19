using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Village : ActionCardBase
    {
        public override string Art => "dom-village";
        public override int Cost => 3;
        
        public override string Text => @"<lines>
            <bold>+1 Card</bold>
            <bold>+2 Actions</bold>
        </lines>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(2);
        }
    }
}