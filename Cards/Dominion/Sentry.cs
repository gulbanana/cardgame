using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Sentry : ActionCardBase
    {
        public override string Art => "dom-sentry";
        public override int Cost => 5;

        public override string Text => @"<lines>
            <bold>+1 Card</bold>
            <bold>+1 Action</bold>
            <run>Look at the top 2 cards of your deck. Trash and/or discard any number of them. Put the rest back on top in any order.</run>
        </lines>";

        protected override async Task ActAsync(IActionHost host)
        {
            var trashes = await host.SelectCards("Choose cards to trash.", Zone.DeckTop2);
            if (trashes.Any())
            {
                host.Trash(trashes, Zone.DeckTop2);
            }

            if (trashes.Length < 2)
            {
                var discards = await host.SelectCards("Choose cards to discard.", trashes.Length == 1 ? Zone.DeckTop1 : Zone.DeckTop2);
                if (discards.Any())
                {
                    host.Discard(discards, Zone.DeckTop2);
                }
                else if (!trashes.Any())
                {
                    var orderedCards = await host.OrderCards("Put these cards back in any order.", Zone.DeckTop2);
                    host.Reorder(orderedCards, Zone.DeckTop2);    
                }   
            }
        }
    }
}