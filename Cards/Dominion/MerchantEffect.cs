using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class MerchantEffect : EffectBase, IReactor
    {
        public override string Art => "Dominion2nd/dom-merchant";
        public override string Text => @"
            <run>The first time you play a Silver this turn,</run>
            <sym prefix='+' suffix='.'>coin1</sym>
        ";

        public async Task<Reaction> ExecuteReactionAsync(IActionHost host, Zone reactFrom, Trigger triggerType, string triggerParameter)
        {
            if (triggerType == Trigger.PlayCard && triggerParameter == nameof(Base.Silver))
            {
                return Reaction.After(() =>
                {
                    host.AddCoins(1);
                    host.RemoveEffect(Name);
                });
            }

            return Reaction.None();
        }
    }
}