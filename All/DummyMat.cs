using Cardgame.API;

namespace Cardgame.All
{
    internal class DummyMat : IMat
    {
        public string Name { get; }
        public string Label => Name;
        public string Art => "island";

        public DummyMat(string name)
        {
            Name = name;
        }

        public string GetContents(string[] cards, bool isOwner)
        {
            return $"<run>{Name}: Mat not found.</run>";
        }
    }
}