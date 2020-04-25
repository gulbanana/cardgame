using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards
{
    public abstract class TreasureCardBase : CardBase, ITreasureCard
    {
        public override CardType[] Types => new[] { CardType.Treasure };
        public override string Text => null;
        public abstract Cost Value { get; }

        public Cost? GetStaticValue()
        {
            return Value;
        }

        public Task<Cost> GetValueAsync(IActionHost host)
        {
            return Task.FromResult(Value);
        }
    }
}