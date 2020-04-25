using System.Collections.Generic;
using System.Linq;
using Cardgame.API;

namespace Cardgame.Cards
{
    // XXX cut-down internal version of All.Cards - only used by Pirate Ship and reaction params. should be possible to get rid of it at some point
    internal static class AllCards
    {
        private static readonly Dictionary<string, ICard> byName;

        static AllCards()
        {
            byName = new Dictionary<string, ICard>();

            var baseType = typeof(ICard);
            var types = typeof(CardBase).Assembly.GetTypes()
                .Where(t => t.IsPublic)
                .Where(t => !t.IsAbstract)
                .Where(t => baseType.IsAssignableFrom(t));

            foreach (var t in types)
            {
                var o = t.GetConstructor(new System.Type[0]).Invoke(new object[0]);
                byName[t.Name] = (ICard)o;
            }
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

        internal sealed class DummyCard : ICard
        {
            public string Name { get; }
            public CardType[] Types => new[] { CardType.Curse };
            public string Art => "curse-2x";
            public string Text => $"<run>{Name}: Card not found.</run>";
            public string HasMat => null;

            public DummyCard(string name)
            {
                Name = name;
            }

            public Cost GetCost(IModifier[] _) => 0;
        }
    }
}