using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class EmbargoToken : ReactionEffectBase, IToken
    {
        public override string Art => "Seaside/Embargo";
        public override Trigger ReactionTrigger => Trigger.BuyCard;
        public string Description => "an Embargo token";

        public override string Text => "When a player buys a card from this pile, they gain a Curse.";

        protected override void React(IActionHost host, string trigger)
        {
            host.Gain("Curse");
        }
    }
}