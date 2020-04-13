namespace Cardgame.Cards
{
    public class Curse : CardModel
    {
        public override CardType Type => CardType.Curse;
        public override string Art => "curse-2x";
        public override int Cost => 0;
        public override TextModel Text => null;
    }
}