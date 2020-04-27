using System;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards
{
    public abstract class ReactionEffectBase : EffectBase, IReactor
    {
        public abstract Trigger ReactionTrigger { get; }
        
        public async Task ExecuteReactionAsync(IActionHost host, Zone reactFrom, Trigger triggerType, string triggerParameter)
        {
            if (triggerType == ReactionTrigger)
            {
                await ReactAsync(host, triggerParameter);
            }
        }

        protected virtual Task ReactAsync(IActionHost host, string trigger)
        {
            React(host, trigger);
            return Task.CompletedTask;
        }

        protected virtual void React(IActionHost host, string trigger) => throw new NotImplementedException($"Reaction not implemented.");
    }
}