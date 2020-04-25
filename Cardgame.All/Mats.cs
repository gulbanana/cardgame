using System.Collections.Generic;
using System.Linq;
using Cardgame.API;

namespace Cardgame.All
{
    public static class Mats
    {
        private static readonly Dictionary<string, IMat> byName;

        static Mats()
        {
            byName = new Dictionary<string, IMat>();

            var baseType = typeof(IMat);
            var types = typeof(Cardgame.Cards.MatBase).Assembly.GetTypes()
                .Where(t => t.IsPublic)
                .Where(t => !t.IsAbstract)
                .Where(t => baseType.IsAssignableFrom(t));

            foreach (var t in types)
            {
                var o = t.GetConstructor(new System.Type[0]).Invoke(new object[0]);
                byName[t.Name] = (IMat)o;
            }
        }

        public static IMat ByName(string id)
        {
            if (byName.ContainsKey(id))
            {
                return byName[id];
            }
            else
            {
                return new DummyMat(id);
            }
        }

        public static bool Exists(string id)
        {
            return byName.ContainsKey(id);
        }
    }
}