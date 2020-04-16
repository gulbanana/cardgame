using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Market : ActionCardModel
    {
        public override string Art => "dom-market";
        public override int Cost => 5;

        public override string Text => @"
        <block>
            <lines>
                <run>+1 Card</run>
                <run>+1 Action</run>
                <run>+1 Buy</run>
                <sym prefix='+'>coin1</sym>
            </lines>
        </block>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(1);
            host.AddBuys(1);
            host.AddMoney(1);
        }
    }
}