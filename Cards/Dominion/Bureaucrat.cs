using System.Linq;
using System.Threading.Tasks;

namespace Cardgame.Cards.Dominion
{
    public class Bureaucrat : ActionCardModel
    {
        public override string SubType => "Attack";
        public override string Art => "dom-bureaucrat";
        public override int Cost => 4;        

        public override string Text => @"<run>
            Gain a Silver onto your deck. Each other player reveals a Victory card from their hand and puts it onto their deck (or reveals a hand with no Victory cards).
        </run>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.Gain("Silver", Zone.DeckTop1);

            await host.Attack(async player =>
            {
                if (player.GetHand().Any(card => card.Type == CardType.Victory))
                {
                    var onTop = await player.SelectCard(
                        "Choose a card to put onto your deck.", 
                        Zone.Hand, 
                        cards => cards.OfType<VictoryCardModel>()
                    );
                    player.RevealAndMove(onTop.Name, Zone.Hand, Zone.DeckTop1);
                }
                else
                {
                    player.RevealAll(Zone.Hand);
                }
            });
        }
    }
}