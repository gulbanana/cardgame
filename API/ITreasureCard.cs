using Cardgame.Shared;

namespace Cardgame.API
{
    public interface ITreasureCard : ICard
    {
        Cost GetValue(IModifier[] modifiers);
    }

    public static class TreasureCardExtensions
    {
        public static Cost GetValue(this ITreasureCard card, IModifierSource modifierSource) => card.GetValue(modifierSource.GetModifiers());
    }
}