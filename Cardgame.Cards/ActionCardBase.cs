using System;
using System.Threading.Tasks;
using Cardgame.API;

namespace Cardgame.Cards
{
    public abstract class ActionCardBase : CardBase, IActionCard
    {
        public override CardType[] Types => new[] { CardType.Action };
        
        public Task ExecuteActionAsync(IActionHost host) => ActAsync(host);

        protected virtual Task ActAsync(IActionHost host)
        {
            Act(host);
            return Task.CompletedTask;
        } 

        protected virtual void Act(IActionHost host) => throw new NotImplementedException($"Card not implemented.");
    }
}