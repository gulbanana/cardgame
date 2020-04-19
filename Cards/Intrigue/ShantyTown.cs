using System.Linq;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class ShantyTown : ActionCardBase
    {
        public override string Art => "int-shanty-town";
        public override int Cost => 3;
        
        public override string Text => @"<paras>
            <bold>+2 Actions</bold>
            <spans>
                <run>Reveal your hand. If you have no Action cards in hand,</run>
                <bold>+2 Cards.</bold>
            </spans>
        </paras>";

        protected override void Act(IActionHost host)
        {
            host.AddActions(2);

            var hand = host.Reveal(Zone.Hand);
            if (!hand.Any(card => card.Types.Contains(CardType.Action)))
            {
                host.DrawCards(2);
            }
        }
    }
}