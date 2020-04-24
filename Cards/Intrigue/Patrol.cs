using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Patrol : ActionCardBase
    {
        public override string Art => "int-patrol";
        public override int Cost => 5;

        public override string Text => @"<paras>
            <bold>+3 Cards</bold>
            <run>Reveal the top 4 card of your deck. Put the Victory cards and Curses into your hand. Put the rest back in any order.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(3);

            var top4 = host.Reveal(Zone.DeckTop(4));
            var victoryOrCurseCards = top4.Where(card => card.Types.Contains(CardType.Victory) || card.Name == "Curse").ToArray();

            if (victoryOrCurseCards.Length > 0)
            {
                host.PutIntoHand(victoryOrCurseCards, Zone.Deck);

                if (victoryOrCurseCards.Length < 3)
                {
                    var zone = Zone.DeckTop(4 - victoryOrCurseCards.Length);
                    var reorderedCards = await host.OrderCards("Put these cards back in any order.", zone);
                    host.Reorder(reorderedCards, zone);
                }
            }
        }
    }
}