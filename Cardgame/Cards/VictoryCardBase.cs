using Cardgame.API;

namespace Cardgame.Cards
{
    public abstract class VictoryCardBase : CardBase, IVictoryCard
    {
        public override CardType[] Types => new[]{ CardType.Victory };
        public override string Text => null;
        public abstract int Score(string[] dominion);
    }
}