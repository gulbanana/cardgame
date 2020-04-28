using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Gardens : VictoryCardBase
    {
        public override string Art => "dom-gardens";
        public override Cost Cost => 4;

        public override string Text => @"
            <run>Worth</run>
            <sym>1vp</sym>
            <run>per 10 cards you have (round down).</run>
        ";

        public override int Score(string[] dominion) 
        {
            return dominion.Length / 10;
        }
    }
}