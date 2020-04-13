using System;
using System.Threading.Tasks;

namespace Cardgame.Cards
{
    public abstract class ActionCardModel : CardModel
    {
        public override CardType Type => CardType.Action;
        public virtual string SubType => null;
        
        public virtual Task PlayAsync(IActionHost host) => Task.Run(() => Play(host));

        protected virtual void Play(IActionHost host) => throw new NotImplementedException($"{Name}: card not yet implemented.");
    }
}