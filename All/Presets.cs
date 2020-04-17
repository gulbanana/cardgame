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
                { "Deck Top", new[]{ "Artisan", "Bureaucrat", "CouncilRoom", "Festival", "Harbinger", "Laboratory", "Moneylender", "Sentry", "Vassal", "Village" }},
                { "Sleight of Hand", new[]{ "Cellar", "CouncilRoom", "Festival", "Gardens", "Library", "Harbinger", "Militia", "Poacher", "Smithy", "ThroneRoom" }},
                { "Improvements", new[]{ "Artisan", "Cellar", "Market", "Merchant", "Mine", "Moat", "Moneylender", "Poacher", "Remodel", "Witch" }},
                { "Silver & Gold", new[]{ "Bandit", "Bureaucrat", "Chapel", "Harbinger", "Laboratory", "Merchant", "Mine", "Moneylender", "ThroneRoom", "Vassal" }}
            } },
            { CardSet.Intrigue1st, new Dictionary<string, string[]> {
                { "Victory Dance", new[] { "Bridge", "Duke", "GreatHall", "Harem", "Ironworks", "Masquerade", "Nobles", "Pawn", "Scout", "Upgrade" } }
            } }
        };
    }
}