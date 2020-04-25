using System.Collections.Generic;
using System.Linq;
using Cardgame.API;
using Cardgame.Shared;

namespace Cardgame.All
{
    public static class Cards
    {
        private static readonly Dictionary<string, ICard> byName;
        private static readonly Dictionary<string, CardSet?> bySet;
        private static string[] _Cursers;

        static Cards()
        {
            byName = new Dictionary<string, ICard>();
            bySet = new Dictionary<string, CardSet?>();

            var baseType = typeof(ICard);
            var types = typeof(Cards).Assembly.GetTypes()
                .Where(t => t.IsPublic)
                .Where(t => !t.IsAbstract)
                .Where(t => baseType.IsAssignableFrom(t));

            foreach (var t in types)
            {
                var o = t.GetConstructor(new System.Type[0]).Invoke(new object[0]);
                byName[t.Name] = (ICard)o;

                var firstEditions = new[] { 
                    "Adventurer", "Chancellor", "Feast", "Spy", "Thief", "Woodcutter",
                    "Coppersmith", "GreatHall", "Saboteur", "Scout", "SecretChamber", "Tribute"
                };
                bySet[t.Name] = t.Namespace switch {
                    "Cardgame.Cards.Dominion" when firstEditions.Contains(t.Name) => CardSet.Dominion1st,
                    "Cardgame.Cards.Dominion" => CardSet.Dominion2nd,
                    "Cardgame.Cards.Intrigue" when firstEditions.Contains(t.Name) => CardSet.Intrigue1st,
                    "Cardgame.Cards.Intrigue" => CardSet.Intrigue2nd,
                    "Cardgame.Cards.Seaside" => CardSet.Seaside,
                    _ => null
                };
            }

            // XXX build from models?
            _Cursers = new[] { "Witch", "Torturer", "Replace", "Embargo", "SeaHag" };
        }

        public static ICard ByName(string id)
        {
            if (byName.ContainsKey(id))
            {
                return byName[id];
            }
            else
            {
                return new DummyCard(id);
            }
        }

        public static ICard ByName(Instance instance)
        {
            return ByName(instance.Id);
        }

        public static string GetColor(CardType type)
        {
            switch (type)
            {
                case CardType.Action:
                case CardType.Attack:
                    return "action";

                case CardType.Duration:
                    return "duration";

                case CardType.Reaction:
                    return "reaction";

                case CardType.Treasure:
                    return "treasure";

                case CardType.Victory:
                    return "victory";

                case CardType.Curse:
                default:
                    return "curse";
            }
        }

        public static bool Exists(string id)
        {
            return byName.ContainsKey(id);
        }

        public static string[] Base()
        {
            return new[]{"Estate", "Duchy", "Province", "Copper", "Silver", "Gold", "Curse"};
        }

        public static CardSet? GetSet(string id)
        {
            if (bySet.ContainsKey(id))
            {
                return bySet[id];
            }
            else
            {
                return null;
            }
        }

        public static bool UsesCurse(string id)
        {
            return _Cursers.Contains(id);
        }

        public static IEnumerable<IGrouping<CardSet, string>> BySet()
        {
            return from exemplar in byName.Values
                   let set = GetSet(exemplar.Name)
                   where set.HasValue
                   group exemplar.Name by set.Value into g
                   select g;
        }
    }
}