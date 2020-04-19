using Cardgame.Shared;

namespace Cardgame.Cards
{
    public abstract class ModifierEffectBase : EffectBase, IModifier
    {
        public virtual int ReduceCardCost => 0;
        public virtual int IncreaseTreasureValue(string id) => 0;
    }
}