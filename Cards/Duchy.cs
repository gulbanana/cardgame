namespace Cardgame.Cards
{
    public class Duchy : BaseCardModel
    {
        public override CardType Type => CardType.Victory;
        public override string Art => "duchy-2x";
        public override int Cost => 5;
    }
}