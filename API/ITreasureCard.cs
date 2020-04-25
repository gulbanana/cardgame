using System.Linq;
using System.Threading.Tasks;
using Cardgame.Shared;

namespace Cardgame.API
{
    public interface ITreasureCard : ICard
    {
        Cost? GetStaticValue();
        Task<Cost> GetValueAsync(IActionHost host);
    }

    public static class TreasureCardExtensions
    {
        public static Cost? GetModifiedValue(this ITreasureCard card, IModifier[] modifiers) 
        {
            var value = card.GetStaticValue();
            if (!value.HasValue)
            {
                return value;
            }
            else
            {
                return new Cost(value.Value.Coins + modifiers.Select(m => m.IncreaseTreasureValue(card.Name)).Sum(), value.Value.Potion);
            }
        }

        public static Cost? GetModifiedValue(this ITreasureCard card, IModifierSource modifierSource) => card.GetModifiedValue(modifierSource.GetModifiers());
        
    }
}