using System.Collections.Generic;
using System.Linq;

namespace Cardgame.Cards
{
    public static class All
    {
        public static readonly Dictionary<string, CardModel> ByName;

        static All()
        {
            ByName = new Dictionary<string, CardModel>();

            var baseType = typeof(CardModel);
            var types = typeof(All).Assembly.GetTypes()
                .Where(t => t.IsPublic)
                .Where(t => !t.IsAbstract)
                .Where(t => baseType.IsAssignableFrom(t));

            foreach (var t in types)
            {
                var o = t.GetConstructor(new System.Type[0]).Invoke(new object[0]);
                ByName[t.Name] = (CardModel)o;
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

                case CardType.Action when subType == "Reaction":
                    return "reaction";

                default:
                    return "action";
            }
        }
    }
}