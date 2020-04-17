using Cardgame.API;

namespace Cardgame.Cards
{
    public abstract class CardBase : ICard
    {
        public string Name { get; protected set; }
        public abstract CardType Type { get; }
        public abstract string Art { get; }
        public abstract string Text { get; }
        public abstract int Cost { get; }

        public CardBase()
        {
            Name = this.GetType().Name;
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