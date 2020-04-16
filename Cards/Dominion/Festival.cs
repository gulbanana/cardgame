using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Festival : ActionCardModel
    {
        public override string Art => "dom-festival";
        public override int Cost => 5;

        public override string Text => @"
        <block>
            <lines>
                <run>+2 Actions</run>
                <run>+1 Buy</run>
                <sym prefix='+'>coin2</sym>
            </lines>
        </block>";

        protected override void Act(IActionHost host)
        {
            host.AddActions(2);
            host.AddBuys(1);
            host.AddMoney(2);
        }
    }
}