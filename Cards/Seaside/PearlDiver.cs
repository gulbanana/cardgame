using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class PearlDiver : ActionCardBase
    {
        public override Cost Cost => 2;

        public override string Text => @"<paras>
            <lines>
                <bold>+1 Card</bold>
                <bold>+1 Action</bold>
            </lines>
            <run>Look at the bottom card of your deck. You may put it on top.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.DrawCards(1);
            host.AddActions(1);

            var bottomCard = host.Examine(Zone.DeckBottom).SingleOrDefault();
            if (bottomCard != null && await host.YesNo("Pearl Diver", $"<run>Put</run><card>{bottomCard.Name}</card><run>on top of your deck?</run>"))
            {
                host.PutOnDeck(bottomCard, Zone.DeckBottom);
            }
        }
    }
}