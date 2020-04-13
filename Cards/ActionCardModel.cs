using System;
using System.Threading.Tasks;

namespace Cardgame.Cards
{
    public abstract class ActionCardModel : CardModel
    {
        public override CardType Type => CardType.Action;
        public virtual string SubType => null;
        
        public virtual Task PlayAsync() => Task.Run(Play);

        protected virtual Task Play() => throw new NotImplementedException($"{Name}: card not yet implemented.");
    }
}