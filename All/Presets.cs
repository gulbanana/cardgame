using System.Collections.Generic;
using Cardgame.Shared;

namespace Cardgame.All
{
    public static class Presets
    {
        public static Dictionary<CardSet, Dictionary<string, string[]>> BySet = new Dictionary<CardSet, Dictionary<string, string[]>>
        {
            { CardSet.Dominion1st, new Dictionary<string, string[]> {
                { "First Game", new[]{ "Cellar", "Market", "Militia", "Mine", "Moat", "Remodel", "Smithy", "Village", "Woodcutter", "Workshop" }},
                { "Big Money", new[]{ "Adventurer", "Bureaucrat", "Chancellor", "Chapel", "Feast", "Laboratory", "Market", "Mine", "Moneylender", "ThroneRoom" }},
                { "Interaction", new[]{ "Bureaucrat", "Chancellor", "CouncilRoom", "Festival", "Library", "Militia", "Moat", "Spy", "Thief", "Village" }},
                { "Size Distortion", new[]{ "Cellar", "Chapel", "Feast", "Gardens", "Laboratory", "Thief", "Village", "Witch", "Woodcutter", "Workshop" }},
                { "Village Square", new[]{ "Bureaucrat", "Cellar", "Festival", "Library", "Market", "Remodel", "Smithy", "ThroneRoom", "Village", "Woodcutter" }}
            } },
            { CardSet.Dominion2nd, new Dictionary<string, string[]> {
                { "First Game", new[]{ "Cellar", "Market", "Merchant", "Militia", "Mine", "Moat", "Remodel", "Smithy", "Village", "Workshop" }},
                { "Size Distortion", new[]{ "Artisan", "Bandit", "Bureaucrat", "Chapel", "Festival", "Gardens", "Sentry", "ThroneRoom", "Witch", "Workshop" }},
            } }
        };
    }
}