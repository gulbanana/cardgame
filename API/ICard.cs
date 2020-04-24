using System;
using System.Collections.Generic;
using System.Linq;
using Cardgame.Shared;

namespace Cardgame.API
{
    public interface ICard
    {
        string Name { get; }
        CardType[] Types { get; }
        string Art { get; }
        string Text { get; }
        string HasMat { get; }
        int GetCost(IModifier[] modifiers);
    }

    public static class CardExtensions
    {
        public static int GetCost(this ICard card, IModifierSource modifierSource) => card.GetCost(modifierSource?.GetModifiers() ?? Array.Empty<IModifier>());

        public static int SortByTypes(this ICard card)
        {
            return card.Types.Length switch
            {
                1 => card.Types[0] switch
                {
                    CardType.Action => 10,
                    CardType.Treasure => 20,
                    CardType.Victory => 30,
                    CardType.Curse => 40
                },
                2 when card.Types.Contains(CardType.Action) && card.Types.Contains(CardType.Reaction) => 9,
                2 when card.Types.Contains(CardType.Action) && card.Types.Contains(CardType.Attack) => 11,
                2 when card.Types.Contains(CardType.Action) && card.Types.Contains(CardType.Duration) => 12,
                2 when card.Types.Contains(CardType.Action) && card.Types.Contains(CardType.Treasure) => 15,
                2 when card.Types.Contains(CardType.Action) && card.Types.Contains(CardType.Victory) => 16,
                2 when card.Types.Contains(CardType.Treasure) && card.Types.Contains(CardType.Victory) => 25,
                _ => 100
            };
        }

        public static string[] Names(this IEnumerable<ICard> source)
        {
            return source.Select(card => card.Name).ToArray();
        }
    }
}