using System.Linq;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Adventurer : ActionCardBase
    {
        public override string Art => "dom-adventurer";
        public override int Cost => 6;
        
        public override string Text => @"<lines>
            <run>Reveal cards from your deck until you reveal 2 Treasure cards.</run>
            <run>Put those Treasure cards into your hand and discard the other revealed cards.</run>
        </lines>";

        protected override void Act(IActionHost host)
        {
            var foundTreasures = 0;            
            var reshuffles = host.ShuffleCount;

            while (foundTreasures < 2 && host.ShuffleCount - reshuffles < 2)
            {
                var top1 = host.Reveal(Zone.DeckTop1).SingleOrDefault();
                if (top1 != null)
                {
                    if (top1.Types.Contains(CardType.Treasure))
                    {
                        foundTreasures++;
                        host.PutIntoHand(top1, Zone.DeckTop1);
                    }
                    else
                    {
                        host.Discard(top1, Zone.DeckTop1);
                    }
                }
            }
        }
    }
}