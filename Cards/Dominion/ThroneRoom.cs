using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class ThroneRoom : ActionCardModel
    {
        public override string Art => "dom-throne-room";
        public override int Cost => 4;
        
        public override string Text => @"<run>
            You may play an Action card from your hand twice.
        </run>";

        protected override async Task ActAsync(IActionHost host)
        {
            var doubledCard = await host.SelectCard("Choose an Action card.", Zone.Hand, cards => cards.OfType<IActionCard>());
            if (doubledCard != null)
            {
                host.PlayAction(doubledCard, Zone.Hand);
                host.PlayAction(doubledCard, Zone.InPlay);
            }
        }
    }
}