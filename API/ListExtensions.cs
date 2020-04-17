using System;
using System.Collections.Generic;
using System.Linq;

namespace Cardgame.API
{
    public static class ListExtensions
    {
        public static IEnumerable<T> Without<T>(this IEnumerable<T> source, IEnumerable<T> removals)
        {
            var all = source.ToList();
            foreach (var removal in removals)
            {
                all.Remove(removal);
            }
            return all;
        }
    }
}