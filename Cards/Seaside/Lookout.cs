using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class Lookout : ActionCardBase
    {
        public override int Cost => 3;

        public override string Text => @"<paras>
            <bold>+1 Action</bold>
            <run>Look at the top 3 cards of your deck. Trash one of them. Discard one of them. Put the other one back on top of your deck.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.AddActions(1);

            var trashed = await host.SelectCard("Choose a card to trash.", Zone.DeckTop(3));
            if (trashed != null)
            {
                host.Trash(trashed, Zone.Deck);

                var discarded = await host.SelectCard("Choose a card to discard.", Zone.DeckTop(2));
                if (discarded != null)
                {
                    host.Discard(discarded, Zone.Deck);
                }
            }
        }
    }
}