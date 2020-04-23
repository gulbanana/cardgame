using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class Caravan : DurationCardBase
    {
        public override int Cost => 4;

        public override string Text => @"<paras>
            <lines>
                <bold>+1 Card</bold>
                <bold>+1 Action</bold>
            </lines>
            <spans>
                <run>At the start of your next turn,</run>
                <bold>+1 Card.</bold>
            </spans>
        </paras>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(1);
        }

        protected override void OnBeginTurn(IActionHost host)
        {
            host.DrawCards(1);
        }
    }
}