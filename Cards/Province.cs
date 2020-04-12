namespace Cardgame.Cards
{
    public class Province : BaseCardModel
    {
        public override CardType Type => CardType.Victory;
        public override string Art => "province-2x";
        public override int Cost => 8;
    }
}