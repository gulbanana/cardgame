using System;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards
{
    public abstract class ReactionCardBase : ActionCardBase, IReactor
    {
        public override CardType[] Types => new[] { CardType.Action, CardType.Reaction };
        public abstract Trigger ReactionTrigger { get; }

        public async Task<Reaction> ExecuteReactionAsync(IActionHost host, Zone reactFrom, Trigger triggerType, string triggerParameter)
        {
            if (reactFrom == Zone.Hand && triggerType == ReactionTrigger)
            {
                return await ReactAsync(host, triggerParameter);                        
            }
            else
            {
                return Reaction.None();
            }
        }

        protected virtual Task<Reaction> ReactAsync(IActionHost host, string trigger) => Task.FromResult(React(host, trigger));

        protected virtual Reaction React(IActionHost host, string trigger) => throw new NotImplementedException($"Reaction not implemented.");
    }
}