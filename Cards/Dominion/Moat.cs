using System.Threading.Tasks;

namespace Cardgame.Cards
{
    public class Moat : ActionCardModel
    {
        public override string SubType => "Reaction";
        public override TriggerType ReactionTrigger => TriggerType.Attack;
        public override string Art => "dom-moat";
        public override int Cost => 2;        

        public override string Text => @"
        <paras>
            <block>
                <run>+2 Cards</run>
            </block>
            <run>When another player plays an Attack card, you may reveal this from your hand. If you do, you are unaffected by that attack.</run>
        </paras>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(2);
        }

        protected override async Task<Reaction> ReactAsync(IActionHost host)
        {
            var shouldReact = await host.YesNo("Reveal Moat from your hand?");
            if (shouldReact)
            {
                return Reaction.Cancel();
            }
            else
            {
                return Reaction.None();
            }
        }
    }
}