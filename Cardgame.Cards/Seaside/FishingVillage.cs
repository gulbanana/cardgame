using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class FishingVillage : DurationCardBase
    {
        public override Cost Cost => 3;

        public override string Text => @"<paras>
            <lines>
                <bold>+2 Actions</bold>
                <bold><sym>+coin1</sym></bold>
            </lines>
            <spans>
                <run>At the start of your next turn:</run>
                <bold>+1 Action</bold>
                <run>and</run>
                <bold><sym>+coin1.</sym></bold>
            </spans>
        </paras>";

        protected override void Act(IActionHost host)
        {
            host.AddActions(2);
            host.AddCoins(1);
        }

        protected override void OnBeginTurn(IActionHost host)
        {
            host.AddActions(1);
            host.AddCoins(1);
        }
    }
}