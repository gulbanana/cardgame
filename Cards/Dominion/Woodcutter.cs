using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Woodcutter : ActionCardBase
    {
        public override int Cost => 3;

        public override string Text => @"<bold>
            <lines>
                <run>+1 Buy</run>
                <sym prefix='+'>coin2</sym>
            </lines>
        </bold>";

        protected override void Act(IActionHost host)
        {
            host.AddBuys(1);
            host.AddMoney(2);
        }
    }
}