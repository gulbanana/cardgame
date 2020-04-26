using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards
{
    /// <summary>Actions which have an effect when something happens to the card itself.</summary>
    /// <remarks>ActionCardBase could do this, but we don't want needless IReactors for the engine to grub through.</remarks>
    public abstract class TriggeredActionCardBase : ActionCardBase, IReactor
    {
        public async Task<Reaction> ExecuteReactionAsync(IActionHost host, Zone reactFrom, Trigger triggerType, string triggerParameter)
        {
            if (reactFrom == Zone.This)
            {
                if (triggerType == Trigger.DiscardFromPlay)
                {
                    return Reaction.After(() => OnDiscardedAsync(host));
                }
            }

            return Reaction.None();
        }

        protected abstract Task OnDiscardedAsync(IActionHost host);
    }
}