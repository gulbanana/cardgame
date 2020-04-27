using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards
{
    public abstract class DurationCardBase : ActionCardBase, IReactor
    {
        public override CardType[] Types => new[] { CardType.Action, CardType.Duration } ;

        public Task ExecuteReactionAsync(IActionHost host, Zone reactFrom, Trigger triggerType, string triggerParameter)
        {
            if (reactFrom == Zone.InPlay)
            {
                if (triggerType == Trigger.BeginTurn && triggerParameter == host.Player)
                {
                    OnBeginTurn(host);
                    host.CompleteDuration();
                }
                else if (triggerType == Trigger.BeforePlayCard)
                {
                    var card = AllCards.ByName(triggerParameter);
                    OnBeforePlayCard(host, card);
                }
                else if (triggerType == Trigger.AfterPlayCard)
                {
                    var card = AllCards.ByName(triggerParameter);
                    OnAfterPlayCard(host, card);
                }
            }

            return Task.CompletedTask;
        }

        protected virtual void OnBeginTurn(IActionHost host) { }
        protected virtual void OnBeforePlayCard(IActionHost host, ICard card) { }
        protected virtual void OnAfterPlayCard(IActionHost host, ICard card) { }
    }
}