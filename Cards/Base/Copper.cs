using Cardgame.API;

namespace Cardgame.Cards.Base
{
    public class Copper : TreasureCardBase
    {
        public override string Art => "copper-2x";
        public override Cost Cost => 0;
        public override Cost Value => 1;
    }
}