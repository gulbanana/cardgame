using System;
using System.Collections.Generic;
using System.Linq;
using Cardgame.Shared;

namespace Cardgame.All
{
    public static class Presets
    {
        private static Dictionary<CardSet, Dictionary<string, string[]>> bySet = new Dictionary<CardSet, Dictionary<string, string[]>>
        {
            { CardSet.Dominion1st, new Dictionary<string, string[]> {
                { "First Game", new[]{ "Cellar", "Market", "Militia", "Mine", "Moat", "Remodel", "Smithy", "Village", "Woodcutter", "Workshop" }},
                { "Big Money", new[]{ "Adventurer", "Bureaucrat", "Chancellor", "Chapel", "Feast", "Laboratory", "Market", "Mine", "Moneylender", "ThroneRoom" }},
                { "Interaction", new[]{ "Bureaucrat", "Chancellor", "CouncilRoom", "Festival", "Library", "Militia", "Moat", "Spy", "Thief", "Village" }},
                { "Size Distortion", new[]{ "Cellar", "Chapel", "Feast", "Gardens", "Laboratory", "Thief", "Village", "Witch", "Woodcutter", "Workshop" }},
                { "Village Square", new[]{ "Bureaucrat", "Cellar", "Festival", "Library", "Market", "Remodel", "Smithy", "ThroneRoom", "Village", "Woodcutter" }},
            } },
            { CardSet.Dominion2nd, new Dictionary<string, string[]> {
                { "First Game", new[]{ "Cellar", "Market", "Merchant", "Militia", "Mine", "Moat", "Remodel", "Smithy", "Village", "Workshop" }},
                { "Size Distortion", new[]{ "Artisan", "Bandit", "Bureaucrat", "Chapel", "Festival", "Gardens", "Sentry", "ThroneRoom", "Witch", "Workshop" }},
                { "Deck Top", new[]{ "Artisan", "Bureaucrat", "CouncilRoom", "Festival", "Harbinger", "Laboratory", "Moneylender", "Sentry", "Vassal", "Village" }},
                { "Sleight of Hand", new[]{ "Cellar", "CouncilRoom", "Festival", "Gardens", "Library", "Harbinger", "Militia", "Poacher", "Smithy", "ThroneRoom" }},
                { "Improvements", new[]{ "Artisan", "Cellar", "Market", "Merchant", "Mine", "Moat", "Moneylender", "Poacher", "Remodel", "Witch" }},
                { "Silver & Gold", new[]{ "Bandit", "Bureaucrat", "Chapel", "Harbinger", "Laboratory", "Merchant", "Mine", "Moneylender", "ThroneRoom", "Vassal" }},
            } },
            { CardSet.Intrigue1st, new Dictionary<string, string[]> {
                { "Victory Dance", new[] { "Bridge", "Duke", "GreatHall", "Harem", "Ironworks", "Masquerade", "Nobles", "Pawn", "Scout", "Upgrade" } },
                { "Secret Schemes", new[] { "Conspirator", "Harem", "Ironworks", "Pawn", "Saboteur", "ShantyTown", "Steward", "Swindler", "TradingPost", "Tribute" } },
                { "Best Wishes", new[] { "Coppersmith", "Courtyard", "Masquerade", "Scout", "ShantyTown", "Steward", "Torturer", "TradingPost", "Upgrade", "WishingWell" } }
            } },
            { CardSet.Intrigue2nd, new Dictionary<string, string[]> {
                // { "Victory Dance", new[] { "Baron", "Courtier", "Duke", "Harem", "Ironworks", "Masquerade", "Mill", "Nobles", "Patrol", "Replace" } },
                // { "The Plot Thickens", new[] { "Conspirator", "Ironworks", "Lurker", "Pawn", "MiningVillage", "SecretPassage", "Steward", "Swindler", "Torturer", "TradingPost" } },
                { "Best Wishes", new[] { "Baron", "Conspirator", "Courtyard", "Diplomat", "Duke", "SecretPassage", "ShantyTown", "Torturer", "Upgrade", "WishingWell" } },
            } }
        };

        public static IReadOnlyDictionary<string, string[]> BySet(CardSet set)
        {
            if (!bySet.ContainsKey(set))
            {
                return new Dictionary<string, string[]>();
            }
            else
            {
                return bySet[set].ToDictionary(kvp => kvp.Key, kvp =>
                {
                    var byCost = kvp.Value.Select(All.Cards.ByName).OrderBy(card => card.GetCost(Array.Empty<IModifier>())).Select(card => card.Name).ToArray();
                    return new[] { byCost[0], byCost[2], byCost[4], byCost[6], byCost[8], byCost[1], byCost[3], byCost[5], byCost[7], byCost[9] };
                });
            }
        }
    }
}