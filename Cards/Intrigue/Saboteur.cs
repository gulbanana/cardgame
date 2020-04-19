using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Saboteur : ActionAttackCardBase
    {
        public override string Art => "int-saboteur";
        public override int Cost => 5;

        public override string Text => @"<small>
            <run>Each other player reveals cards from the top of their deck until revealing one costing</run>
            <sym>coin3</sym>
            <run>or more. They trash that card and may gain a card costing at most</run>
            <sym>coin2</sym>
            <run>less than it. They discard the other revealed cards.</run>
        </small>";

        protected override async Task ActAsync(IActionHost host)
        {
            await host.Attack(async target =>
            {
reveal:         var topDeck = target.Reveal(Zone.DeckTop1).SingleOrDefault();
                if (topDeck != null)
                {
                    var topDeckCost = topDeck.GetCost(host);                    
                    if (topDeckCost >= 3)
                    {
                        target.Trash(topDeck, Zone.DeckTop1);
                        var gained = await target.SelectCards("Choose a card to gain, or none.", Zone.SupplyAvailable, card => card.GetCost(host) <= topDeckCost-2, 0, 1);
                        if (gained.Any())
                        {
                            target.Gain(gained.Single());
                        }
                    }
                    else
                    {
                        target.Discard(topDeck, Zone.DeckTop1);
                        if (target.ShuffleCount < 2)
                        {
                            goto reveal;
                        }
                    }
                }
            });
        }
    }
}