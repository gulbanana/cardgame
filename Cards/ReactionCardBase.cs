using System;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards
{
    public abstract class ReactionCardBase : ActionCardBase, IReactor
    {
        public override string SubType => "Reaction";        
        public abstract Trigger ReactionTrigger { get; }

        public async Task<Reaction> ExecuteReactionAsync(IActionHost host, Trigger triggerType, string triggerParameter)
        {
            if (triggerType == ReactionTrigger && await host.YesNo(Name, $@"<spans>
                <run>Reveal</run>
                <card>Moat</card>
                <run>from your hand?</run>
            </spans>"))
            {
                host.Reveal(Name);
                return await ReactAsync(host, triggerParameter);                        
            }
            else
            {
                return Reaction.None();
            }
        }

        protected virtual Task<Reaction> ReactAsync(IActionHost host, string trigger) => Task.FromResult(React(host, trigger));

        protected virtual Reaction React(IActionHost host, string trigger)=> throw new NotImplementedException($"Reaction not implemented.");
    }
}