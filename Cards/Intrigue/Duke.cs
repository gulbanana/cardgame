using System.Linq;

namespace Cardgame.Cards.Intrigue
{
    public class Duke : VictoryCardBase
    {
        public override string Art => "int-duke";
        public override int Cost => 5;
        public override int Score(string[] dominion) => dominion.Count(id => id == "Duchy");

        public override string Text => @"<spans>
            <run>Worth</run>
            <sym prefix='1'>vp</sym>
            <run>per Duchy you have.</run>
        </spans>";
    }
}