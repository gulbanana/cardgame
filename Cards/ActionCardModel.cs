using System;
using System.Threading.Tasks;

namespace Cardgame.Cards
{
    public abstract class ActionCardModel : CardModel
    {
        public override CardType Type => CardType.Action;
        public virtual string SubType => null;
        public virtual TriggerType ReactionTrigger => TriggerType.None;
        
        public Task ExecuteActionAsync(IActionHost host) => ActAsync(host);

        protected virtual Task ActAsync(IActionHost host)
        {
            Act(host);
            return Task.CompletedTask;
        } 

        protected virtual void Act(IActionHost host) => throw new NotImplementedException($"Card not implemented.");

        public Task<Reaction> ExecuteReactionAsync(IActionHost host, string trigger) => ReactAsync(host, trigger);

        protected virtual Task<Reaction> ReactAsync(IActionHost host, string trigger) => throw new NotImplementedException($"No reaction implemented.");
    }
}