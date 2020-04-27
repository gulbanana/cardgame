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
                else if (triggerType == Trigger.Attack)
                {
                    OnAttack(host, triggerParameter);
                }
                else if (triggerType == Trigger.PlayCard)
                {
                    var card = AllCards.ByName(triggerParameter);
                    OnPlayCard(host, card);
                }
            }

            return Task.CompletedTask;
        }

        protected virtual void OnAttack(IActionHost host, string attacker) { }
        protected virtual void OnBeginTurn(IActionHost host) { }
        protected virtual void OnPlayCard(IActionHost host, ICard card) { }
    }
}