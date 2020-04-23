using Cardgame.API;

namespace Cardgame.Cards
{
    public abstract class AttackCardBase : ActionCardBase
    {
        public override CardType[] Types => new[] { CardType.Action, CardType.Attack } ;
    }
}