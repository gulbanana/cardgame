using Cardgame.API;

namespace Cardgame.Cards
{
    public abstract class ActionAttackCardBase : ActionCardBase
    {
        public override CardType[] Types => new[] { CardType.Action, CardType.Attack } ;
    }
}