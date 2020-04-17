namespace Cardgame.Cards.Base
{
    public class Duchy : VictoryCardBase
    {
        public override string Art => "duchy-2x";
        public override int Cost => 5;
        public override int Score(string[] dominion) => 3;
    }
}