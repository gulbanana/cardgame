using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Seaside
{
    public class Embargo : ActionCardBase
    {
        public override Cost Cost => 2;

        public override string Text => @"<paras>
            <sym>+coin2</sym>
            <run>Trash this. Add an Embargo token to a Supply pile. (For the rest of the game, when a player buys a card from that pile, they gain a Curse.)</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.AddCoins(2);
            host.Trash("Embargo", Zone.InPlay);

            var pile = await host.SelectCard("Choose a Supply pile to embargo.", Zone.SupplyAvailable);
            host.AddToken("EmbargoToken", pile.Name);
        }
    }
}