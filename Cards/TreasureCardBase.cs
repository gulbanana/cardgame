using Cardgame.API;

namespace Cardgame.Cards
{
    public abstract class TreasureCardBase : CardBase, ITreasureCard
    {
        public override CardType Type => CardType.Treasure;
        public override string Text => null;
        public abstract int Value { get; }
    }
}