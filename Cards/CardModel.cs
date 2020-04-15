namespace Cardgame.Cards
{
    public abstract class CardModel
    {
        public string Name { get; protected set; }
        public abstract CardType Type { get; }
        public abstract string Art { get; }
        public abstract string Text { get; }
        public abstract int Cost { get; }

        public CardModel()
        {
            Name = this.GetType().Name;
        }

        public override bool Equals(object obj)
        {
            return obj is CardModel other && this.Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}