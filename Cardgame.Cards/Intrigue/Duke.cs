using System.Linq;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Duke : VictoryCardBase
    {
        public override string Art => "int-duke";
        public override Cost Cost => 5;
        public override int Score(string[] dominion) => dominion.Count(id => id == "Duchy");

        public override string Text => @"
            <run>Worth</run>
            <sym>1vp</sym>
            <run>per Duchy you have.</run>
        ";
    }
}