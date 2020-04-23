using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards
{
    public abstract class DurationCardBase : ActionCardBase, IReactor
    {
        public override CardType[] Types => new[] { CardType.Action, CardType.Duration } ;

        public Task<Reaction> ExecuteReactionAsync(IActionHost host, Zone reactFrom, Trigger triggerType, string triggerParameter)
        {
            if (reactFrom == Zone.InPlay)
            {
                if (triggerType == Trigger.BeginTurn && triggerParameter == host.Player)
                {
                    return Task.FromResult(Reaction.Before(() => 
                    {
                        OnBeginTurn(host);
                        host.CompleteDuration();
                    }));
                }
                else if (triggerType == Trigger.PlayCard)
                {
                    var card = All.Cards.ByName(triggerParameter);
                    return Task.FromResult(Reaction.BeforeAndAfter(() =>
                    {
                        OnBeforePlayCard(host, card);
                    }, () =>
                    {
                        OnAfterPlayCard(host, card);
                    }));
                }
                else
                {
                    return Task.FromResult(Reaction.None());
                }
            }
            else
            {
                return Task.FromResult(Reaction.None());
            }
        }

        protected abstract void OnBeginTurn(IActionHost host);

        protected virtual void OnBeforePlayCard(IActionHost host, ICard card) { }
        protected virtual void OnAfterPlayCard(IActionHost host, ICard card) { }
    }
}