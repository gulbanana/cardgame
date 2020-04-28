using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Alchemy
{
    public class Herbalist : TriggeredActionCardBase
    {
        public override Cost Cost => 2;

        public override string Text => @"<paras>
            <lines>
                <bold>+1 Buy</bold>
                <bold><sym>+coin1</sym></bold>
            </lines>
            <run>When you discard this from play, you may put one of your Treasures from play onto your deck.</run>
        </paras>";

        protected override void Act(IActionHost host)
        {
            host.AddBuys(1);
            host.AddCoins(1);
        }
        
        protected override async Task OnDiscardFromPlayAsync(IActionHost host)
        {
            var playedTreasures = host.Examine(Zone.RecentPlays).Any(card => card.Types.Contains(CardType.Treasure));
            if (playedTreasures)
            {
                var putBack = await host.SelectCards(
                    "Choose a Treasure, or none, to put back onto your deck.", 
                    Zone.InPlay,
                    card => card.Types.Contains(CardType.Treasure),
                    0, 1
                );

                host.PutOnDeck(putBack, Zone.InPlay);
            }
        }
    }
}