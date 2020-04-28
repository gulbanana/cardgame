using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Market : ActionCardBase
    {
        public override string Art => "dom-market";
        public override Cost Cost => 5;

        public override string Text => @"<bold>
            <lines>
                <run>+1 Card</run>
                <run>+1 Action</run>
                <run>+1 Buy</run>
                <sym>+coin1</sym>
            </lines>
        </bold>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(1);
            host.AddBuys(1);
            host.AddCoins(1);
        }
    }
}