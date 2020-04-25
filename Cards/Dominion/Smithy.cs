using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Smithy : ActionCardBase
    {
        public override string Art => "dom-smithy";
        public override Cost Cost => 4;

        public override string Text => @"<bold>+3 Cards</bold>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(3);
        }
    }
}