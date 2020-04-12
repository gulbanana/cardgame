namespace Cardgame.Cards
{
    public abstract class KingdomCardModel : CardModel
    {
        public override CardType Type => CardType.Action;
        public virtual string SubType => null;
        
        public void Play() => throw new System.NotImplementedException("Card not yet implemented.");
    }
}