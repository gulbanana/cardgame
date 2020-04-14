namespace Cardgame.Cards
{
    public abstract class VictoryCardModel : CardModel
    {
        public override CardType Type => CardType.Victory;
        public override string Text => null;
        public abstract int VictoryPoints { get; }
    }
}