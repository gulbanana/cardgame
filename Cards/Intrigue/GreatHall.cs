using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class GreatHall : ActionCardBase, IVictoryCard
    {
        public override CardType[] Types => new[] { CardType.Action, CardType.Victory };
        public override string Art => "int-great-hall";
        public override int Cost => 3;

        public override string Text => @"<block>
            <split>
                <lines>
                    <run>+1 Card</run>
                    <run>+1 Action</run>
                </lines>
                <sym large='true' prefix='1'>vp</sym>
            </split>
        </block>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(1);
        }

        public int Score(string[] dominion) => 1;
    }
}