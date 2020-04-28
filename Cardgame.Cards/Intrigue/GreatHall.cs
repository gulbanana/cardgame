using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class GreatHall : VictoryActionCardBase
    {
        public override Cost Cost => 3;
        public override int Score => 1;

        public override string Text => @"<bold>
            <split>
                <lines>
                    <run>+1 Card</run>
                    <run>+1 Action</run>
                </lines>
                <sym large='true'>1vp</sym>
            </split>
        </bold>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(1);
        }
    }
}