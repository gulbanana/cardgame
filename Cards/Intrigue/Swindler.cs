using System.Linq;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Intrigue
{
    public class Swindler : ActionAttackCardBase
    {
        public override string Art => "int-swindler";
        public override int Cost => 3;

        public override string Text => @"<paras>
            <bold><sym prefix='+'>coin2</sym></bold>
            <run>Each other player trashes the top card of their deck and gains a card with the same cost that you choose.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.AddCoins(2);

            await host.Attack(async target =>
            {
                var topDeck = target.Examine(Zone.DeckTop1).Single();
                var topDeckCost = topDeck.GetCost(host);

                target.Trash(topDeck, Zone.DeckTop1);

                var gained = await host.SelectCard(
                    $"Choose a card for {target.Player} to gain.", 
                    Zone.SupplyAvailable, 
                    card => card.GetCost(host) == topDeckCost
                );
                target.Gain(gained);                
            });
        }
    }
}