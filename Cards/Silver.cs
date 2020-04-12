namespace Cardgame.Cards
{
    public class Silver : BaseCardModel
    {
        public override CardType Type => CardType.Treasure;
        public override string Art => "silver-2x";
        public override int Cost => 3;
    }
}