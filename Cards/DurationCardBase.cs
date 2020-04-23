using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards
{
    public abstract class DurationCardBase : ActionCardBase, IReactor
    {
        public override CardType[] Types => new[] { CardType.Action, CardType.Duration } ;

        public Task<Reaction> ExecuteReactionAsync(IActionHost host, Zone reactFrom, Trigger triggerType, string triggerParameter)
        {
            if (reactFrom == Zone.InPlay && triggerType == Trigger.BeginTurn && triggerParameter == host.Player)
            {
                return Task.FromResult(Reaction.Before(() => 
                {
                    NextTurn(host);
                    host.CompleteDuration();
                }));
            }
            else
            {
                return Task.FromResult(Reaction.None());
            }
        }

        protected abstract void NextTurn(IActionHost host);
    }
}