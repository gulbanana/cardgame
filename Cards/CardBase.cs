using System;
using System.Linq;
using Cardgame.API;
using Cardgame.Shared;

namespace Cardgame.Cards
{
    public abstract class CardBase : ICard
    {
        public string Name { get; protected set; }
        public abstract CardType[] Types { get; }
        public abstract string Art { get; }
        public abstract string Text { get; }
        public abstract int Cost { get; }

        public CardBase()
        {
            Name = this.GetType().Name;
        }

        public int GetCost(IModifier[] modifiers)
        {
            return Math.Max(0, Cost - modifiers.Select(m => m.ReduceCardCost).Sum());
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