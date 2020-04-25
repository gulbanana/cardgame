using Cardgame.API;

namespace Cardgame.Cards.Base
{
    public class Silver : TreasureCardBase
    {
        public override string Art => "silver-2x";
        public override Cost Cost => 3;
        public override Cost Value => 2;
    }
}