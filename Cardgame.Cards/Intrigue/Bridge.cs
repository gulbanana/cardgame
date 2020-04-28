using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Bridge : ActionCardBase
    {
        public override string Art => "int-bridge";
        public override Cost Cost => 4;
        
        public override string Text => @"<paras>
            <bold>
                <lines>
                    <run>+1 Buy</run>
                    <sym>+coin1</sym>
                </lines>
            </bold>
            <spans>
                <run>This turn, cards (everywhere) cost</run>
                <sym>coin1</sym>
                <run>less, but not less than</run>
                <sym>coin0.</sym>
            </spans>
        </paras>";

        protected override void Act(IActionHost host)
        {
            host.AddBuys(1);
            host.AddCoins(1);
            host.AddEffect(nameof(BridgeEffect));
        }
    }
}