using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class Bazaar : ActionCardBase
    {
        public override Cost Cost => 5;

        public override string Text => @"<bold>
            <lines>
                <run>+1 Card</run>
                <run>+2 Actions</run>
                <sym>+coin1</sym>
            </lines>
        </bold>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(2);
            host.AddCoins(1);
        }
    }
}