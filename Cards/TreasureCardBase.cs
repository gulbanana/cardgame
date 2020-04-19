using System.Linq;
using Cardgame.API;
using Cardgame.Shared;

namespace Cardgame.Cards
{
    public abstract class TreasureCardBase : CardBase, ITreasureCard
    {
        public override CardType[] Types => new[] { CardType.Treasure };
        public override string Text => null;
        public abstract int Value { get; }

        public int GetValue(IModifier[] modifiers)
        {
            return Value + modifiers.Select(m => m.IncreaseTreasureValue(Name)).Sum();
        }
    }
}