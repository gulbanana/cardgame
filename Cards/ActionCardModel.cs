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

        protected virtual Task ActAsync(IActionHost host) => Task.Run(() => Act(host));

        protected virtual void Act(IActionHost host) => throw new NotImplementedException($"{Name}: card not implemented.");

        public Task<Reaction> ExecuteReactionAsync(IActionHost host) => ReactAsync(host);

        protected virtual Task<Reaction> ReactAsync(IActionHost host) => throw new NotImplementedException($"{Name}: no reaction implemented.");
    }
}