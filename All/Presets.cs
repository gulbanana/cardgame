using System;
using System.Collections.Generic;
using System.Linq;
using Cardgame.Shared;

namespace Cardgame.All
{
    public static class Presets
    {
        private static readonly List<(string name, CardSet[] sets, string[] cards)> all = new List<(string name, CardSet[] sets, string[] cards)>();

        static Presets()
        {
            void preset1(string title, CardSet set, params string[] cards) { all.Add((title, new[]{ set }, cards.ToArray())); }
            void preset2(string title, CardSet set1, CardSet set2, params string[] cards) { all.Add((title, new[]{ set1, set2 }, cards.ToArray())); }

            preset1("First Game (1e)", CardSet.Dominion1st, "Cellar", "Market", "Militia", "Mine", "Moat", "Remodel", "Smithy", "Village", "Woodcutter", "Workshop");
            preset1("Big Money (1e)", CardSet.Dominion1st, "Adventurer", "Bureaucrat", "Chancellor", "Chapel", "Feast", "Laboratory", "Market", "Mine", "Moneylender", "ThroneRoom");
            preset1("Interaction (1e)", CardSet.Dominion1st, "Bureaucrat", "Chancellor", "CouncilRoom", "Festival", "Library", "Militia", "Moat", "Spy", "Thief", "Village");
            preset1("Size Distortion (1e)", CardSet.Dominion1st, "Cellar", "Chapel", "Feast", "Gardens", "Laboratory", "Thief", "Village", "Witch", "Woodcutter", "Workshop");
            preset1("Village Square (1e)", CardSet.Dominion1st, "Bureaucrat", "Cellar", "Festival", "Library", "Market", "Remodel", "Smithy", "ThroneRoom", "Village", "Woodcutter");

            preset1("First Game", CardSet.Dominion2nd, "Cellar", "Market", "Merchant", "Militia", "Mine", "Moat", "Remodel", "Smithy", "Village", "Workshop");
            preset1("Size Distortion", CardSet.Dominion2nd, "Artisan", "Bandit", "Bureaucrat", "Chapel", "Festival", "Gardens", "Sentry", "ThroneRoom", "Witch", "Workshop");
            preset1("Deck Top", CardSet.Dominion2nd, "Artisan", "Bureaucrat", "CouncilRoom", "Festival", "Harbinger", "Laboratory", "Moneylender", "Sentry", "Vassal", "Village");
            preset1("Sleight of Hand", CardSet.Dominion2nd, "Cellar", "CouncilRoom", "Festival", "Gardens", "Library", "Harbinger", "Militia", "Poacher", "Smithy", "ThroneRoom");
            preset1("Improvements", CardSet.Dominion2nd, "Artisan", "Cellar", "Market", "Merchant", "Mine", "Moat", "Moneylender", "Poacher", "Remodel", "Witch");
            preset1("Silver & Gold", CardSet.Dominion2nd, "Bandit", "Bureaucrat", "Chapel", "Harbinger", "Laboratory", "Merchant", "Mine", "Moneylender", "ThroneRoom", "Vassal");

            preset1("Victory Dance (1e)", CardSet.Intrigue1st, "Bridge", "Duke", "GreatHall", "Harem", "Ironworks", "Masquerade", "Nobles", "Pawn", "Scout", "Upgrade");
            preset1("Secret Schemes (1e)", CardSet.Intrigue1st, "Conspirator", "Harem", "Ironworks", "Pawn", "Saboteur", "ShantyTown", "Steward", "Swindler", "TradingPost", "Tribute");
            preset1("Best Wishes (1e)", CardSet.Intrigue1st, "Coppersmith", "Courtyard", "Masquerade", "Scout", "ShantyTown", "Steward", "Torturer", "TradingPost", "Upgrade", "WishingWell");
            preset2("Deconstruction (1e)", CardSet.Intrigue1st, CardSet.Dominion1st, "Bridge", "MiningVillage", "Remodel", "Saboteur", "SecretChamber", "Spy", "Swindler", "Thief", "ThroneRoom", "Torturer");
            preset2("Hand Madness (1e)", CardSet.Intrigue1st, CardSet.Dominion1st, "Bureaucrat", "Chancellor", "CouncilRoom", "Courtyard", "Mine", "Militia", "Minion", "Nobles", "Steward", "Torturer");
            preset2("Underlings (1e)", CardSet.Intrigue1st, CardSet.Dominion1st, "Baron", "Cellar", "Festival", "Library", "Masquerade", "Minion", "Nobles", "Pawn", "Steward", "Witch");

            preset1("Victory Dance", CardSet.Intrigue2nd, "Baron", "Courtier", "Duke", "Harem", "Ironworks", "Masquerade", "Mill", "Nobles", "Patrol", "Replace");
            preset1("The Plot Thickens", CardSet.Intrigue2nd, "Conspirator", "Ironworks", "Lurker", "Pawn", "MiningVillage", "SecretPassage", "Steward", "Swindler", "Torturer", "TradingPost");
            preset1("Best Wishes", CardSet.Intrigue2nd, "Baron", "Conspirator", "Courtyard", "Diplomat", "Duke", "SecretPassage", "ShantyTown", "Torturer", "Upgrade", "WishingWell");
            preset2("Deconstruction", CardSet.Intrigue2nd, CardSet.Dominion2nd, "Diplomat", "Harem", "Lurker", "Replace", "Swindler", "Bandit", "Mine", "Remodel", "ThroneRoom", "Village");
            preset2("Grand Scheme", CardSet.Intrigue2nd, CardSet.Dominion2nd, "Bridge", "Mill", "MiningVillage", "Patrol", "ShantyTown", "Artisan", "CouncilRoom", "Market", "Militia", "Workshop");
            preset2("Underlings", CardSet.Intrigue2nd, CardSet.Dominion2nd, "Courtier", "Diplomat", "Minion", "Nobles", "Pawn", "Cellar", "Festival", "Library", "Sentry", "Vassal");
        }

        public static IReadOnlyDictionary<string, string[]> BySet(CardSet set)
        {
            if (!all.Any(t => t.sets.Contains(set)))
            {
                return new Dictionary<string, string[]>();
            }
            else
            {
                return all
                    .Where(t => t.sets.Contains(set))
                    .ToDictionary(t => t.name, t =>
                    {
                        return t.cards
                            .Select(All.Cards.ByName)
                            .OrderBy(card => card.GetCost(Array.Empty<IModifier>()))
                            .Select(card => card.Name).ToArray();
                    });
            }
        }

        public static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string[]>> GroupedBySet(CardSet set)
        {
            string FormatSet(CardSet s)
            {
                return s.ToString().Replace("1st", "").Replace("2nd", "");
            }

            string FormatSets(CardSet[] sets)
            {
                if (sets.Length == 1)
                {
                    return $"{FormatSet(sets[0])} only";
                }
                else
                {
                    return string.Join(" & ", sets.OrderBy(s => s != set).ThenBy(s => s).Select(FormatSet));
                }
            }

            if (!all.Any(t => t.sets.Contains(set)))
            {
                return new Dictionary<string, IReadOnlyDictionary<string, string[]>>();
            }
            else
            {
                return all
                    .Where(t => t.sets.Contains(set))
                    .GroupBy(t => FormatSets(t.sets))
                    .ToDictionary(g => g.Key, g => g.ToDictionary(t => t.name, t =>
                    {
                        return t.cards
                            .Select(All.Cards.ByName)
                            .OrderBy(card => card.GetCost(Array.Empty<IModifier>()))
                            .Select(card => card.Name)
                            .ToArray();
                    }) as IReadOnlyDictionary<string, string[]>);
            }
        }
    }
}