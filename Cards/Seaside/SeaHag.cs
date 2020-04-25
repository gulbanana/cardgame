using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class SeaHag : AttackCardBase
    {
        public override Cost Cost => 4;

        public override string Text => "Each other player discards the top card of their deck, then gains a Curse onto their deck.";

        protected override async Task ActAsync(IActionHost host)
        {
            await host.Attack(target =>
            {
                target.Discard(Zone.DeckTop(1));
                target.Gain("Curse", Zone.Deck);
            });
        }
    }
}