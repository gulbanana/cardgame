using System;
using System.Threading.Tasks;

namespace Cardgame.Cards
{
    public abstract class ActionCardModel : CardModel
    {
        public override CardType Type => CardType.Action;
        public virtual string SubType => null;
        
        public Task ExecuteAsync(IActionHost host) => PlayAsync(host);

        protected virtual Task PlayAsync(IActionHost host) => Task.Run(() => Play(host));

        protected virtual void Play(IActionHost host) => throw new NotImplementedException($"{Name}: card not yet implemented.");
    }
}