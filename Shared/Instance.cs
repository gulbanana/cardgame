using System.Collections.Generic;
using System.Linq;

namespace Cardgame.Shared
{
    public struct Instance
    {
        private static readonly Dictionary<string, int> counts = new Dictionary<string, int>();

        public static Instance Of(string id)
        {
            if (!counts.ContainsKey(id)) counts[id] = 0;
            return new Instance { Id = id, Counter = counts[id]++ };
        }

        public string Id { get; set; }
        public int Counter { get; set; }

        public override string ToString()
        {
            return $"{Id}:{Counter}";
        }

        public override bool Equals(object obj)
        {
            return obj is Instance other && other.Id == this.Id && other.Counter == this.Counter;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ Counter;
        }
    }

    public static class InstanceExtensions
    {
        public static string[] Names(this IEnumerable<Instance> source)
        {
            return source.Select(i => i.Id).ToArray();
        }

        public static bool Contains(this IEnumerable<Instance> source, string id)
        {
            return source.Any(i => i.Id == id);
        }

        public static Instance Extract(this List<Instance> source, string id)
        {
            var extracted = source.Find(e => e.Id == id);
            source.Remove(extracted);
            return extracted;
        }

        public static Instance ExtractLast(this List<Instance> source, string id)
        {
            var extractedIndex = source.FindLastIndex(e => e.Id == id);
            var extracted = source[extractedIndex];
            source.RemoveAt(extractedIndex);
            return extracted;
        }
    }
}