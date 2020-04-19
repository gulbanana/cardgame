using Cardgame.API;

namespace Cardgame.Cards
{
    public abstract class ActionVictoryCardBase : ActionCardBase, IVictoryCard
    {
        public override CardType[] Types => new[] { CardType.Action, CardType.Victory };
        protected virtual int Score { get; }

        int IVictoryCard.Score(string[] dominion)
        {
            return Score;
        }
    }
}