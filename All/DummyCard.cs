using Cardgame.API;
using Cardgame.Shared;

namespace Cardgame.All
{
    internal class DummyCard : ICard
    {
        public string Name { get; }
        public CardType[] Types => new[] { CardType.Curse };
        public string Art => "dom-village";
        public string Text => $"<run>{Name}: Card not found.</run>";

        public DummyCard(string name)
        {
            Name = name;
        }

        public int GetCost(IModifier[] _) => 0;
    }
}