using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class ThroneRoom : ActionCardBase
    {
        public override string Art => "dom-throne-room";
        public override Cost Cost => 4;
        
        public override string Text => @"You may play an Action card from your hand twice.";

        protected override async Task ActAsync(IActionHost host)
        {
            var doubledCard = await host.SelectCard("Choose an Action card.", Zone.Hand, cards => cards.OfType<IActionCard>());
            if (doubledCard != null)
            {
                await host.PlayCard(doubledCard, Zone.Hand);
                await host.PlayCard(doubledCard, Zone.InPlay);
            }
        }
    }
}