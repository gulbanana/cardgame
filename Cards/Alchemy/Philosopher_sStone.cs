using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards.Alchemy
{
    public class Philosopher_sStone : CardBase, ITreasureCard
    {
        public override CardType[] Types => new[] { CardType.Treasure };
        public override Cost Cost => new Cost(3, true);

        public override string Text => @"
            <run>When you play this, count your deck and discard pile. Worth</run>
            <sym>coin1</sym>
            <run>per 5 cards total between them (round down).</run>
        ";

        public Cost? GetStaticValue() => null;

        public async Task<Cost> GetValueAsync(IActionHost host)
        {
            var total = host.Count(Zone.Deck) + host.Count(Zone.Discard);
            return total / 5;
        }
    }
}