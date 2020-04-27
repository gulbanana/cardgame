using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Alchemy
{
    public class Alchemist : TriggeredActionCardBase
    {
        public override Cost Cost => new Cost(3, true);

        public override string Text => @"<paras>
            <lines>
                <bold>+2 Cards</bold>
                <bold>+1 Action</bold>
            </lines>
            <run>When you discard this from play, if you have a Potion in play, you may put this onto your deck.</run>
        </paras>";

        protected override void Act(IActionHost host)
        {
            host.DrawCards(2);
            host.AddActions(1);
        }
        
        protected override async Task OnDiscardFromPlayAsync(IActionHost host)
        {
            // XXX RAW you choose discard order, but we elide that for UI reasons. in practice we trigger all IReactor discards first
            var playedPotion = host.Examine(Zone.InPlay).Any(card => card.Name == "Potion");
            if (playedPotion && await host.YesNo("Alchemist", "<run>Put</run><card>Alchemist</card><run>back onto your deck?</run>"))
            {
                host.PutOnDeck("Alchemist", Zone.Discard);
            }
        }
    }
}