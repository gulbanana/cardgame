using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Merchant : ActionCardBase
    {
        public override string Art => "dom-merchant";
        public override int Cost => 3;
        
        public override string Text => @"<paras>
            <lines>
                <bold>+1 Card</bold>
                <bold>+1 Action</bold>
            </lines>
            <spans>
                <run>The first time you play a Silver this turn,</run>
                <sym prefix='+' suffix='.'>coin1</sym>
            </spans>
        </paras>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(1);
            host.AddEffect(nameof(MerchantEffect));
        }
    }
}