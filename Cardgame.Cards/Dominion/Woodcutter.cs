using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Woodcutter : ActionCardBase
    {
        public override Cost Cost => 3;

        public override string Text => @"<bold>
            <lines>
                <run>+1 Buy</run>
                <sym>+coin2</sym>
            </lines>
        </bold>";

        protected override void Act(IActionHost host)
        {
            host.AddBuys(1);
            host.AddCoins(2);
        }
    }
}