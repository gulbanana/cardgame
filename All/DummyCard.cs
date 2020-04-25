using Cardgame.API;

namespace Cardgame.All
{
    internal class DummyCard : ICard
    {
        public string Name { get; }
        public CardType[] Types => new[] { CardType.Curse };
        public string Art => "curse-2x";
        public string Text => $"<run>{Name}: Card not found.</run>";
        public string HasMat => null;

        public DummyCard(string name)
        {
            Name = name;
        }

        public Cost GetCost(IModifier[] _) => 0;
    }
}