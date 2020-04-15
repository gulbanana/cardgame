namespace Cardgame.Cards.Base
{
    public class Province : VictoryCardModel
    {
        public override string Art => "province-2x";
        public override int Cost => 8;
        public override int Score(string[] dominion) => 6;
    }
}