using Cardgame.Shared;

namespace Cardgame.API
{
    public interface ITreasureCard : ICard
    {
        int GetValue(IModifier[] modifiers);
    }

    public static class TreasureCardExtensions
    {
        public static int GetValue(this ITreasureCard card, IModifierSource modifierSource) => card.GetValue(modifierSource.GetModifiers());
    }
}