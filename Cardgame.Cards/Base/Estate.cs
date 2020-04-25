using Cardgame.API;

namespace Cardgame.Cards.Base
{
    public class Estate : VictoryCardBase
    {
        public override string Art => "estate-2x";
        public override Cost Cost => 2;
        public override int Score(string[] dominion) => 1;
    }
}