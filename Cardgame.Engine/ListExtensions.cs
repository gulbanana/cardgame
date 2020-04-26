using System;
using System.Collections.Generic;
using System.Linq;

namespace Cardgame.Engine
{
    internal static class ListExtensions
    {
        public static void Shuffle<T>(this List<T> source)
        {
            var rng = new Random();
            var temp = new List<T>();
            while (source.Any())
            {
                var i = rng.Next(source.Count);
                var e = source[i];
                temp.Add(e);
                source.RemoveAt(i);
            }
            source.AddRange(temp);
        }
    }
}