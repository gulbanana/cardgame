using Cardgame.Shared;

namespace Cardgame.API
{
    public interface ICard
    {
        string Name { get; }
        CardType Type { get; }
        string Art { get; }
        string Text { get; }
        int GetCost(IModifier[] modifiers);
    }
}