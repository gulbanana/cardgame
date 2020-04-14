using System;
using System.Collections.Generic;
using System.Linq;

namespace Cardgame
{
    public static class ListExtensions
    {
        public static void Shuffle(this List<string> source)
        {
            var rng = new Random();
            var temp = new List<string>();
            while (source.Any())
            {
                var i = rng.Next(source.Count);
                var e = source[i];
                temp.Add(e);
                source.RemoveAt(i);
            }
            source.AddRange(temp);
        }

        public static IEnumerable<T> Without<T>(this IEnumerable<T> source, IEnumerable<T> removals)
        {
            var all = source.ToList();
            foreach (var element in removals)
            {
                var index = all.FindIndex(e => element.Equals(e));
                all.RemoveAt(index);
            }
            return all;
        }
    }
}