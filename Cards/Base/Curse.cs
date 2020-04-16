using Cardgame.API;

namespace Cardgame.Cards.Base
{
    public class Curse : CardModel
    {
        public override CardType Type => CardType.Curse;
        public override string Art => "curse-2x";
        public override int Cost => 0;
        public override string Text => null;
    }
}