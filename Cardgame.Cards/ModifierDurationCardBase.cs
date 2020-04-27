using Cardgame.API;

namespace Cardgame.Cards
{
    public abstract class ModifierDurationCardBase : DurationCardBase, IModifier
    {
        public virtual int ReduceCardCost => 0;
        public virtual int IncreaseTreasureValue(string id) => 0;
        public virtual int? NextHandSize => null;
    }
}