using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Smithy : ActionCardModel
    {
        public override string Art => "dom-smithy";
        public override int Cost => 4;

        public override string Text => @"
        <block>
            <run>+3 Cards</run>
        </block>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(3);
        }
    }
}