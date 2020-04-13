namespace Cardgame.Cards
{
    public abstract class CardModel
    {
        public string Name { get; }
        public abstract CardType Type { get; }
        public abstract string Art { get; }
        public abstract TextModel Text { get; }
        public abstract int Cost { get; }

        public CardModel()
        {
            Name = this.GetType().Name;
        }
    }
}