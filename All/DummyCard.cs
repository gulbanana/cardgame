using Cardgame.API;

namespace Cardgame.All
{
    internal class DummyCard : ICard
    {
        public string Name { get; }
        public CardType Type => CardType.Victory;
        public string Art => "dom-village";
        public int Cost => 0;
        public string Text => $"<run>{Name}: Card not found.</run>";

        public DummyCard(string name)
        {
            Name = name;
        }
    }
}