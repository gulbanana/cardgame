using System;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards
{
    public abstract class ReactionCardModel : ActionCardModel, IReactor
    {
        public override string SubType => "Reaction";        
        public abstract TriggerType ReactionTrigger { get; }

        public async Task<Reaction> ExecuteReactionAsync(IActionHost host, TriggerType triggerType, string triggerParameter)
        {
            if (triggerType == ReactionTrigger)
            {
                return await ReactAsync(host, triggerParameter);
            }
            else
            {
                return Reaction.None();
            }
        }

        protected virtual Task<Reaction> ReactAsync(IActionHost host, string trigger) => throw new NotImplementedException($"No reaction implemented.");
    }
}