using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Moat : ReactionCardBase
    {
        public override Trigger ReactionTrigger => Trigger.Attack;
        public override string Art => "dom-moat";
        public override int Cost => 2;        

        public override string Text => @"<split>
            <block>
                <run>+2 Cards</run>
            </block>
            <run>When another player plays an Attack card, you may first reveal this from your hand, to be unaffected by it.</run>
        </split>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(2);
        }

        protected override Reaction React(IActionHost host, string trigger)
        {
            return Reaction.Cancel();
        }
    }
}