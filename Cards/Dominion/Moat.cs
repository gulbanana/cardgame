using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Moat : ActionReactionCardBase
    {
        public override Trigger ReactionTrigger => Trigger.PlayCard;
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

        protected override async Task<Reaction> ReactAsync(IActionHost host, string trigger)
        {
            if (!host.IsActive && All.Cards.ByName(trigger).Types.Contains(CardType.Attack) && await host.YesNo("Moat", $@"<spans>
                <run>Reveal</run>
                <card>Moat</card>
                <run>from your hand?</run>
            </spans>"))
            {
                host.Reveal("Moat");
                return Reaction.BeforeAndAfter(
                    () => host.PreventAttack(true),
                    () => host.PreventAttack(false)
                );
            }
            else
            {
                return Reaction.None();
            }
        }
    }
}