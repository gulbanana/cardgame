using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards
{
    /// <summary>Actions which have an effect when something happens to the card itself.</summary>
    /// <remarks>ActionCardBase could do this, but we don't want needless IReactors for the engine to grub through.</remarks>
    public abstract class TriggeredActionCardBase : ActionCardBase, IReactor
    {
        public async Task ExecuteReactionAsync(IActionHost host, Zone reactFrom, Trigger triggerType, string triggerParameter)
        {
            if (reactFrom == Zone.This)
            {
                if (triggerType == Trigger.DiscardFromPlay)
                {
                    await OnDiscardFromPlayAsync(host);
                }
            }
        }

        protected abstract Task OnDiscardFromPlayAsync(IActionHost host);
    }
}