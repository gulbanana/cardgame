using Cardgame.API;

namespace Cardgame.Cards
{
    public abstract class TreasureCardModel : CardModel, ITreasureCard
    {
        public override CardType Type => CardType.Treasure;
        public override string Text => null;
        public abstract int Value { get; }
    }
}