using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class EmbargoToken : ReactionEffectBase, IToken
    {
        public override string Art => "Seaside/Embargo";
        public override Trigger ReactionTrigger => Trigger.BuyCard;
        public string Description => "an Embargo token";

        public override string Text => "When a player buys a card from this pile, they gain a Curse.";

        protected override async Task ReactAsync(IActionHost host, string trigger)
        {
            await host.Gain("Curse");
        }
    }
}