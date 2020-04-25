using Cardgame.API;

namespace Cardgame.Cards.Base
{
    public class Gold : TreasureCardBase
    {
        public override string Art => "gold-2x";
        public override Cost Cost => 6;
        public override Cost Value => 3;
    }
}