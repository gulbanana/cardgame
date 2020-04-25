using System;
using System.Collections.Generic;
using System.Linq;
using Cardgame.API;
using Cardgame.Shared;

namespace Cardgame.All
{
    public static class Presets
    {
        public static readonly List<(string name, CardSet[] sets, string[] cards)> List = new List<(string name, CardSet[] sets, string[] cards)>();

        static Presets()
        {
            void preset1(string title, CardSet set, params string[] cards) { List.Add((title, new[]{ set }, cards.ToArray())); }
            void preset2(string title, CardSet set1, CardSet set2, params string[] cards) { List.Add((title, new[]{ set1, set2 }, cards.ToArray())); }

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

            preset1("High Seas", CardSet.Seaside, "Bazaar", "Caravan", "Embargo", "Explorer", "Haven", "Island", "Lookout", "PirateShip", "Smugglers", "Wharf");
            preset1("Buried Treasure", CardSet.Seaside, "Ambassador", "Cutpurse", "FishingVillage", "Lighthouse", "Outpost", "PearlDiver", "Tactician", "TreasureMap", "Warehouse", "Wharf");
            preset1("Shipwrecks", CardSet.Seaside, "GhostShip", "MerchantShip", "NativeVillage", "Navigator", "PearlDiver", "Salvager", "SeaHag", "Smugglers", "Treasury", "Warehouse");
            preset2("Reach for Tomorrow", CardSet.Seaside, CardSet.Dominion2nd, "Artisan", "Cellar", "CouncilRoom", "Vassal", "Village", "Cutpurse", "GhostShip", "Lookout", "SeaHag", "TreasureMap");
            preset2("Repetition", CardSet.Seaside, CardSet.Dominion2nd, "Festival", "Harbinger", "Militia", "Workshop", "Caravan", "Explorer", "Outpost", "PearlDiver", "PirateShip", "Treasury");
            preset2("Give and Take", CardSet.Seaside, CardSet.Dominion2nd, "Library", "Market", "Moneylender", "Witch", "Ambassador", "FishingVillage", "Haven", "Island", "Salvager", "Smugglers");
            preset2("A Star to Steer By", CardSet.Seaside, CardSet.Intrigue2nd, "SecretPassage", "Diplomat", "Swindler", "WishingWell", "Courtier", "Lookout", "TreasureMap", "GhostShip", "Haven", "Outpost");
            preset2("Shore Patrol", CardSet.Seaside, CardSet.Intrigue2nd, "Patrol", "Replace", "ShantyTown", "TradingPost", "Pawn", "Island", "Wharf", "Cutpurse", "Lighthouse", "Warehouse");
            preset2("Bridge Crossing", CardSet.Seaside, CardSet.Intrigue2nd, "Lurker", "Nobles", "Duke", "Conspirator", "Bridge", "Salvager", "Embargo", "Smugglers", "NativeVillage", "Treasury");

            preset2("Forbidden Arts", CardSet.Alchemy, CardSet.Dominion2nd, "Apprentice", "Familiar", "Possession", "University", "Bandit", "Cellar", "CouncilRoom", "Gardens", "Laboratory", "ThroneRoom");
            preset2("Potion Mixers", CardSet.Alchemy, CardSet.Dominion2nd, "Alchemist", "Apothecary", "Golem", "Herbalist", "Transmute", "Cellar", "Festival", "Militia", "Poacher", "Smithy");
            preset2("Chemistry Lesson", CardSet.Alchemy, CardSet.Dominion2nd, "Alchemist", "Golem", "Philosopher_sStone", "University", "Bureaucrat", "Market", "Moat", "Remodel", "Vassal", "Witch");
        }

        public static IReadOnlyDictionary<string, string[]> BySet(CardSet set)
        {
            if (!List.Any(t => t.sets.Contains(set)))
            {
                return new Dictionary<string, string[]>();
            }
            else
            {
                return List
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

            if (!List.Any(t => t.sets.Contains(set)))
            {
                return new Dictionary<string, IReadOnlyDictionary<string, string[]>>();
            }
            else
            {
                return List
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