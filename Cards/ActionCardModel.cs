using System;
using System.Threading.Tasks;

namespace Cardgame.Cards
{
    public abstract class ActionCardModel : CardModel
    {
        public override CardType Type => CardType.Action;
        public virtual string SubType => null;
        
        public Task ExecuteAsync(IActionHost host) => ActAsync(host);

        protected virtual Task ActAsync(IActionHost host) => Task.Run(() => Act(host));

        protected virtual void Act(IActionHost host) => throw new NotImplementedException($"{Name}: card not yet implemented.");
    }
}