using System.Linq;
using Cardgame.API;
using Cardgame.Shared;

namespace Cardgame.Cards
{
    public abstract class TreasureCardBase : CardBase, ITreasureCard
    {
        public override CardType[] Types => new[] { CardType.Treasure };
        public override string Text => null;
        public abstract Cost Value { get; }

        public Cost GetValue(IModifier[] modifiers)
        {
            return new Cost(Value.Coins + modifiers.Select(m => m.IncreaseTreasureValue(Name)).Sum(), Value.Potion);
        }
    }
}