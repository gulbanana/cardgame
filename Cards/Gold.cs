namespace Cardgame.Cards
{
    public class Gold : BaseCardModel
    {
        public override CardType Type => CardType.Treasure;
        public override string Art => "gold-2x";
        public override int Cost => 6;
    }
}