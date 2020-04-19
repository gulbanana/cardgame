using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Bureaucrat : ActionCardBase
    {
        public override CardType[] Types => new[] { CardType.Action, CardType.Attack };
        public override string Art => "dom-bureaucrat";
        public override int Cost => 4;        

        public override string Text => @"Gain a Silver onto your deck. Each other player reveals a Victory card from their hand and puts it onto their deck (or reveals a hand with no Victory cards).";

        protected override async Task ActAsync(IActionHost host)
        {
            host.Gain("Silver", Zone.DeckTop1);

            await host.Attack(async player =>
            {
                var hand = player.Examine(Zone.Hand);
                if (hand.Any(card => card.Types.Contains(CardType.Victory)))
                {
                    var onTop = await player.SelectCard(
                        "Choose a card to put onto your deck.", 
                        Zone.Hand, 
                        cards => cards.OfType<IVictoryCard>()
                    );
                    player.Reveal(onTop, Zone.Hand);
                    player.PlaceOnDeck(onTop, Zone.Hand);
                }
                else
                {
                    player.Reveal(hand, Zone.Hand);
                }
            });
        }
    }
}