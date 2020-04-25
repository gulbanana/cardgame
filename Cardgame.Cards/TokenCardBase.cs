using Cardgame.API;

namespace Cardgame.Cards
{
    public abstract class TokenBase : CardBase
    {
        public override CardType[] Types => new CardType[0];
        public override string Art => null;
        public override Cost Cost => 0;
    }
}