using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Ironworks : ActionCardBase
    {
        public override string Art => "int-ironworks";
        public override Cost Cost => 4;

        public override string Text => @"<paras>
            <lines>
                <spans>
                    <run>Gain a card costing up to</run>
                    <sym>coin4.</sym>
                </spans>
                <run>If the gained card is an...</run>
            </lines>
            <lines>
                <spans>
                    <run>Action card,</run>
                    <bold>+1 Action</bold>
                </spans>
                <spans>
                    <run>Treasure card,</run>
                    <bold><sym>+coin1</sym></bold>
                </spans>
                <spans>
                    <run>Victory card,</run>
                    <bold>+1 Card</bold>
                </spans>
            </lines>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            var gained = await host.SelectCard("Choose a card to gain.", Zone.SupplyAvailable, card => card.GetCost(host).LessThanOrEqual(4));
            await host.Gain(gained);

            if (gained is IActionCard) host.AddActions(1);
            if (gained is ITreasureCard) host.AddCoins(1);
            if (gained is IVictoryCard) host.DrawCards(1);
        }
    }
}