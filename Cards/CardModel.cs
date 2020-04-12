namespace Cardgame.Cards
{
    public abstract class CardModel
    {
        public abstract CardType Type { get; }
        public abstract string Art { get; }
        public abstract TextModel Text { get; }
    }
}