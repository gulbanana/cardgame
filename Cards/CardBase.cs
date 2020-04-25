using System;
using System.Linq;
using Cardgame.API;
using Cardgame.Shared;

namespace Cardgame.Cards
{
    public abstract class CardBase : ICard
    {
        public string Name { get; protected set; }
        public virtual string Art => Name;
        public abstract CardType[] Types { get; }
        public abstract string Text { get; }
        public virtual string HasMat => null;
        public abstract Cost Cost { get; }

        public CardBase()
        {
            Name = this.GetType().Name;
        }

        public Cost GetCost(IModifier[] modifiers)
        {
            return new Cost(Math.Max(0, Cost.Coins - modifiers.Select(m => m.ReduceCardCost).Sum()), Cost.Potion);
        }

        public override bool Equals(object obj)
        {
            return obj is ICard other && this.Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}