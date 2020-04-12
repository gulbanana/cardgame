namespace Cardgame.Cards
{
    public abstract class TreasureCardModel : CardModel
    {
        public override CardType Type => CardType.Treasure;
        public override TextModel Text => null;
        public abstract int Value { get; }
    }
}