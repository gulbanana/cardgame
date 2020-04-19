using System.Collections.Generic;
using System.Linq;
using Cardgame.API;

namespace Cardgame.Widgets
{
    internal static class Backgrounds
    {
        public static string FromTypes(IEnumerable<CardType> types)
        {
            var colours = types.Select(All.Cards.GetColor);
            return FromColours(colours);
        }

        public static string FromColours(IEnumerable<string> colours)
        {
            if (colours.Distinct().Count() == 1)
            {
                return $"var(--card-type-{colours.First()})";
            }
            else if (colours.Count() == 2)
            {
                if (colours.Contains("action") && colours.Contains("reaction"))
                {
                    return $"var(--card-type-reaction)";
                }
                else
                {
                    return $@"linear-gradient(var(--card-type-{colours.First()}) 0%, 
                                              var(--card-type-{colours.First()}) 40%, 
                                              var(--card-type-{colours.Last()}) 60%, 
                                              var(--card-type-{colours.Last()}) 100%)";
                }
            }
            else // give up
            {
                return $"var(--card-type-{colours.First()}";
            }
        }
    }
}