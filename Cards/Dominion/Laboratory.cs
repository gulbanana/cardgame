using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Laboratory : ActionCardBase
    {
        public override string Art => "dom-laboratory";
        public override int Cost => 5;
        
        public override string Text => @"
        <block>
            <lines>
                <run>+2 Cards</run>
                <run>+1 Action</run>
            </lines>
        </block>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(2);
            host.AddActions(1);
        }
    }
}