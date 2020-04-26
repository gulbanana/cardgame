using System.Collections.Generic;
using System.Linq;
using Cardgame.API;

namespace Cardgame.UI.Widgets
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
            if (!colours.Any())
            {
                return "var(--card-type-special)";
            }
            else if (colours.Distinct().Count() == 1)
            {
                return $"var(--card-type-{colours.First()})";
            }
            else if (colours.Count() == 2)
            {
                if (colours.Contains("action") && colours.Contains("reaction"))
                {
                    return $"var(--card-type-reaction)";
                }
                else if (colours.Contains("action") && colours.Contains("duration"))
                {
                    return $"var(--card-type-duration)";
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