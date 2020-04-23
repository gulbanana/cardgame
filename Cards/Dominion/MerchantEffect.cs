using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class MerchantEffect : ReactionEffectBase
    {
        public override string Art => "Dominion2nd/dom-merchant";
        public override Trigger ReactionTrigger => Trigger.PlayCard;
        
        public override string Text => @"
            <run>The first time you play a Silver this turn,</run>
            <sym prefix='+' suffix='.'>coin1</sym>
        ";

        protected override void React(IActionHost host, string trigger)
        {
            host.AddCoins(1);
            host.RemoveEffect(Name);
        }
    }
}