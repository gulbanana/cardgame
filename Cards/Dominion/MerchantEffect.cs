using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class MerchantEffect : EffectBase, IReactor
    {
        public override string Art => "dom-merchant";
        public override string Text => @"<spans>
            <run>The first time you play a Silver this turn,</run>
            <sym prefix='+' suffix='.'>coin1</sym>
        </spans>";

        public async Task<Reaction> ExecuteReactionAsync(IActionHost host, Trigger trigger, string parameter)
        {
            if (trigger == Trigger.PlayCard && parameter == nameof(Base.Silver))
            {
                return Reaction.After(() =>
                {
                    host.AddMoney(1);
                    host.RemoveEffect(Name);
                });
            }

            return Reaction.None();
        }
    }
}