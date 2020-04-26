using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cardgame.API;

namespace Cardgame.All
{
    public static class Mats
    {
        private static readonly Dictionary<string, IMat> byName;

        static Mats()
        {
            byName = new Dictionary<string, IMat>();
        }

        public static void Init(Assembly implementations)
        {
            var baseType = typeof(IMat);
            var types = implementations.GetTypes()
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

        internal class DummyMat : IMat
        {
            public string Name { get; }
            public string Label => Name;
            public string Art => "island";

            public DummyMat(string name)
            {
                Name = name;
            }

            public string GetContents(string[] cards, bool isOwner)
            {
                return $"<run>{Name}: Mat not found.</run>";
            }
        }
    }
}