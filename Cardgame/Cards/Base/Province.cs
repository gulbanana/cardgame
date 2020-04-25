using Cardgame.API;

namespace Cardgame.Cards.Base
{
    public class Province : VictoryCardBase
    {
        public override string Art => "province-2x";
        public override Cost Cost => 8;
        public override int Score(string[] dominion) => 6;
    }
}