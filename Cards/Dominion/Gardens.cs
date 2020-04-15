namespace Cardgame.Cards.Dominion
{
    public class Gardens : VictoryCardModel
    {
        public override string Art => "dom-gardens";
        public override int Cost => 4;
        public override int Score(string[] dominion) => dominion.Length / 10;

        public override string Text => @"<spans>
            <run>Worth</run>
            <sym prefix='1'>vp</sym>
            <run>per 10 cards you have (round down).</run>
        </spans>";
    }
}