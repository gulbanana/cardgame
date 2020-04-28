using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Dominion
{
    public class Militia : AttackCardBase
    {
        public override string Art => "dom-militia";
        public override Cost Cost => 4;        

        public override string Text => @"
        <paras>
            <bold><sym>+coin2</sym></bold>
            <run>Each other player discards down to 3 cards in hand.</run>
        </paras>";

        protected override async Task ActAsync(IActionHost host)
        {
            host.AddCoins(2);

            await host.Attack(player => player.Count(Zone.Hand) > 3, async player =>
            {
                var n = player.Count(Zone.Hand) - 3;
                var discardedCards = await player.SelectCards(n == 1 ? "Choose a card to discard." : $"Choose {n} cards to discard.", n, n);
                player.Discard(discardedCards);
            });
        }
    }
}