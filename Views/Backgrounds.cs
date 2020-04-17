using System.Collections.Generic;
using System.Linq;
using Cardgame.API;

namespace Cardgame.Views
{
    internal static class Backgrounds
    {
        public static string FromTypes(IEnumerable<CardType> types, string subType)
        {
            var colours = types.Select(t => All.Cards.GetColor(t, subType));
            return FromColours(colours);
        }

        public static string FromColours(IEnumerable<string> colours)
        {
            if (colours.Count() == 1)
            {
                return $"var(--card-type-{colours.Single()})";
            }
            else if (colours.Count() == 2)
            {
                return $"linear-gradient(var(--card-type-{colours.First()}), var(--card-type-{colours.Last()}))";
            }
            else // give up
            {
                return $"var(--card-type-{colours.First()}";
            }
        }
    }
}