using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Alchemy
{
    public class University : ActionCardBase
    {
        public override Cost Cost => new Cost(2, true);

        public override string Text => @"<paras>
            <bold>+2 Actions</bold>
            <spans>
                <run>You may gain an Action card costing up to</run>
                <sym suffix='.'>coin5</sym>
            </spans>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.AddActions(2);

            var gained = await host.SelectCards(
                "Choose a card to gain, or none.",
                Zone.SupplyAvailable,
                card => card.Types.Contains(CardType.Action) && card.GetCost(host).LessThan(6),
                0, 1
            );
            await host.Gain(gained);
        }
    }
}