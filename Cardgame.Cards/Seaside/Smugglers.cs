using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class Smugglers : ActionCardBase
    {
        public override Cost Cost => 3;

        public override string Text => @"<spans>
            <run>Gain a copy of a card costing up to</run>
            <sym>coin6</sym>
            <run>that the player to your right gained on their last turn.</run>
        </spans>";

        protected override async Task ActAsync(IActionHost host)
        {
            var lastGains = host.Examine(Zone.RecentGains, host.GetPlayerToRight()).Where(c => c.GetCost(host).LessThanOrEqual(6));
            
            if (lastGains.Count() == 1)
            {
                await host.Gain(lastGains.Single());
            }
            else if (lastGains.Count() > 1)
            {
                var gainedCopy = await host.SelectCard("Choose a card to gain.", lastGains);
                await host.Gain(gainedCopy);
            }
        }
    }
}