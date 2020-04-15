using System.Collections.Generic;
using System.Linq;

namespace Cardgame.Cards
{
    public static class All
    {
        private static readonly Dictionary<string, CardModel> byName;

        static All()
        {
            byName = new Dictionary<string, CardModel>();

            var baseType = typeof(CardModel);
            var types = typeof(All).Assembly.GetTypes()
                .Where(t => t.IsPublic)
                .Where(t => !t.IsAbstract)
                .Where(t => baseType.IsAssignableFrom(t));

            foreach (var t in types)
            {
                var o = t.GetConstructor(new System.Type[0]).Invoke(new object[0]);
                byName[t.Name] = (CardModel)o;
            }
        }

        public static CardModel ByName(string id)
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

        public static string GetColor(CardType type, string subType)
        {
            switch (type)
            {
                case CardType.Treasure:
                    return "treasure";

                case CardType.Victory:
                    return "victory";

                case CardType.Curse:
                    return "curse";

                case CardType.Action when subType == "Reaction":
                    return "reaction";

                default:
                    return "action";
            }
        }

        public static bool Exists(string id)
        {
            return byName.ContainsKey(id);
        }
    }
}