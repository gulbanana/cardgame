using System.Threading.Tasks;

namespace Cardgame.Cards.Dominion
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
            <run>When another player plays an Attack card, you may first reveal this from your hand, to be unaffected by it.</run>
        </paras>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(2);
        }

        protected override async Task<Reaction> ReactAsync(IActionHost host, string trigger)
        {
            var shouldReveal = await host.YesNo("Moat", $@"<spans>
                <run>Reveal</run>
                <card>Moat</card>
                <run>from your hand to be unaffected by {trigger}'s attack?</run>
            </spans>");

            if (shouldReveal)
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