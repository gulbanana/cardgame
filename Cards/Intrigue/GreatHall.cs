using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class GreatHall : ActionVictoryCardBase
    {
        public override string Art => "int-great-hall";
        public override int Cost => 3;
        protected override int Score => 1;

        public override string Text => @"<bold>
            <split>
                <lines>
                    <run>+1 Card</run>
                    <run>+1 Action</run>
                </lines>
                <sym large='true' prefix='1'>vp</sym>
            </split>
        </bold>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(1);
        }
    }
}