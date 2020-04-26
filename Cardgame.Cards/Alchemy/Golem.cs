using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Alchemy
{
    public class Golem : ActionCardBase
    {
        public override Cost Cost => new Cost(4, true);

        public override string Text => @"
            Reveal cards from your deck until you reveal 2 Action cards other than Golems. Discard the other cards, then play the Action cards in either order.
        ";

        protected override async Task ActAsync(IActionHost host)
        {
            var actions = 0;
            var revealed = host.RevealUntil(card =>
            {
                if (card.Types.Contains(CardType.Action) && card.Name != "Golem") actions++;
                return actions == 2;
            });
            
            var revealedActions = revealed.OfType<IActionCard>().Where(card => card.Name != "Golem").ToArray();
            var revealedInactions = revealed.Without(revealedActions).ToArray();

            host.Discard(revealedInactions, Zone.Revealed);

            // XXX reorder if 2
            foreach (var revealedAction in revealedActions)
            {
                await host.PlayCard(revealedAction, Zone.Revealed);
            }
        }
    }
}