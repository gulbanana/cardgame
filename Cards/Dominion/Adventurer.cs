using System.Linq;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Adventurer : ActionCardModel
    {
        public override string Art => "dom-adventurer";
        public override int Cost => 6;
        
        public override string Text => @"
        <lines>
            <run>Reveal cards from your deck until you reveal 2 Treasure cards.</run>
            <run>Put those Treasure cards into your hand and discard the other revealed cards.</run>
        </lines>";

        protected override void Act(IActionHost host)
        {
            var foundTreasures = 0;            
            var reshuffles = host.ShuffleCount;

            while (foundTreasures < 2 && host.ShuffleCount - reshuffles < 2)
            {
                var revealedCard = host.RevealAll(Zone.DeckTop1).Single();
                if (revealedCard.Type == CardType.Treasure)
                {
                    foundTreasures++;
                    host.Draw(revealedCard.Name);
                }
                else
                {
                    host.Discard(revealedCard.Name, Zone.DeckTop1);
                }
            }
        }
    }
}