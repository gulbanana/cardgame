using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Scout : ActionCardBase
    {
        public override Cost Cost => 4;

        public override string Text => @"<paras>
            <bold>+1 Action</bold>
            <run>Reveal the top 4 cards of your deck. Put the revealed Victory cards into your hand. Put the other cards on top of your deck in any order.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.AddActions(1);

            var top4 = host.Reveal(Zone.DeckTop(4));
            var victoryCards = top4.Where(c => c.Types.Contains(CardType.Victory)).ToArray();
            var nonVictoryCards = top4.Where(c => !c.Types.Contains(CardType.Victory)).ToArray();

            if (victoryCards.Any())
            {
                host.PutIntoHand(victoryCards, Zone.Deck);
            }

            if (nonVictoryCards.Count() > 1)
            {
                var zone = Zone.DeckTop(nonVictoryCards.Count());
                var reorderedCards = await host.OrderCards("Put these cards back in any order.", zone);
                host.Reorder(reorderedCards, zone);
            }
        }
    }
}