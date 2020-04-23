using System.Collections.Generic;
using Cardgame.Shared;

namespace Cardgame.API
{
    public interface IMat
    {
        string Name { get; }
        string Label { get; }
        string Art { get; }
        string GetContents(IReadOnlyList<Instance> cards);
    }
}