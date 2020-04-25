using Cardgame.API;

namespace Cardgame.All
{
    internal class DummyEffect : IEffect
    {
        public string Name { get; }
        public string Art => "dom-village";
        public string Text => $"<run>{Name}: Effect not found.</run>";

        public DummyEffect(string name)
        {
            Name = name;
        }
    }
}