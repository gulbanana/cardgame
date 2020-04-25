using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cardgame.API;

namespace Cardgame.All
{
    public static class Effects
    {
        private static readonly Dictionary<string, IEffect> byName;

        static Effects()
        {
            byName = new Dictionary<string, IEffect>();
        }

        public static void Init(Assembly implementations)
        {
            var baseType = typeof(IEffect);
            var types = implementations.GetTypes()
                .Where(t => t.IsPublic)
                .Where(t => !t.IsAbstract)
                .Where(t => baseType.IsAssignableFrom(t));

            foreach (var t in types)
            {
                var o = t.GetConstructor(new System.Type[0]).Invoke(new object[0]);
                byName[t.Name] = (IEffect)o;
            }
        }

        public static IEffect ByName(string id)
        {
            if (byName.ContainsKey(id))
            {
                return byName[id];
            }
            else
            {
                return new DummyEffect(id);
            }
        }

        public static bool Exists(string id)
        {
            return byName.ContainsKey(id);
        }
    }
}