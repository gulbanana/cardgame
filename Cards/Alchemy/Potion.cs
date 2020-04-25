using Cardgame.API;

namespace Cardgame.Cards.Alchemy
{
    public class Potion : TreasureCardBase
    {
        public override Cost Cost => 4;
        public override Cost Value => new Cost(0, true);
    }
}