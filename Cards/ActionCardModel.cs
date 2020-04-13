using System;

namespace Cardgame.Cards
{
    public abstract class ActionCardModel : CardModel
    {
        public override CardType Type => CardType.Action;
        public virtual string SubType => null;
        
        public void Play() => throw new NotImplementedException("Card not yet implemented.");
    }
}