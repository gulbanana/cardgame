using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Moat : AttackReactionCardBase
    {
        public override string Art => "dom-moat";
        public override int Cost => 2;        

        public override string Text => @"<split>
            <bold>+2 Cards</bold>
            <run>When another player plays an Attack card, you may first reveal this from your hand, to be unaffected by it.</run>
        </split>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(2);
        }

        protected override void BeforeAttack(IActionHost host)
        {
            host.PreventAttack(true);
        }

        protected override void AfterAttack(IActionHost host)
        {
            host.PreventAttack(false);
        }
    }
}