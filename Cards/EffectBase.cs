using Cardgame.API;

namespace Cardgame.Cards
{
    public abstract class EffectBase : IEffect
    {
        public string Name { get; }
        public abstract string Art { get; }
        public abstract string Text { get; }

        public EffectBase()
        {
            Name = this.GetType().Name;
        }
    }
}