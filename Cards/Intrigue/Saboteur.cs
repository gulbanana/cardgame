using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Saboteur : ActionCardBase
    {
        public override CardType[] Types => new[] { CardType.Action, CardType.Attack } ;
        public override string Art => "int-saboteur";
        public override int Cost => 5;

        public override string Text => @"<small>
            <spans>
                <run>Each other player reveals cards from the top of their deck until revealing one costing</run>
                <sym>coin3</sym>
                <run>or more. They trash that card and may gain a card costing at most</run>
                <sym>coin2</sym>
                <run>less than it. They discard the other revealed cards.</run>
            </spans>
        </small>";

        protected override async Task ActAsync(IActionHost host)
        {
            await host.Attack(async target =>
            {
reveal:         var topDeck = target.Examine(Zone.DeckTop1).Single();
                target.Reveal(topDeck, Zone.DeckTop1);

                var topDeckCost = topDeck.GetCost(host.GetModifiers());
                if (topDeckCost >= 3)
                {
                    target.Trash(topDeck, Zone.DeckTop1);
                    var gained = await target.SelectCards(
                        "Choose a card to gain, or none.", 
                        Zone.Supply, 
                        cards => cards.Where(card => card.GetCost(host.GetModifiers()) <= topDeckCost-2),
                        0,
                        1
                    );
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
            });
        }
    }
}