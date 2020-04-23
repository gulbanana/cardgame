using System.Collections.Generic;
using Cardgame.API;
using Cardgame.Shared;

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

        public string GetContents(IReadOnlyList<Instance> cards)
        {
            return $"<run>{Name}: Mat not found.</run>";
        }
    }
}