using System.Linq;
using System.Threading.Tasks;
using Cardgame.Shared;

namespace Cardgame.API
{
    public interface ITreasureCard : ICard
    {
        Cost? StaticValue { get; }
        Task<Cost> GetValueAsync(IActionHost host);
    }

    public static class TreasureCardExtensions
    {
        public static Cost? GetModifiedValue(this ITreasureCard card, IModifier[] modifiers) 
        {
            var value = card.StaticValue;
            if (!value.HasValue)
            {
                return value;
            }
            else
            {
                return new Cost(value.Value.Coins + modifiers.Select(m => m.IncreaseTreasureValue(card.Name)).Sum(), value.Value.Potion);
            }
        }

        public static Cost? GetModifiedValue(this ITreasureCard card, GameModel modifierSource) => card.GetModifiedValue(modifierSource.GetModifiers());        
        public static Cost? GetModifiedValue(this ITreasureCard card, IActionHost modifierSource) => card.GetModifiedValue(modifierSource.GetModifiers());        
    }
}