using System.Collections.Generic;
using Cardgame.Model;

namespace Cardgame.Engine
{
    internal class TurnRecord
    {
        public readonly List<Instance> Buys = new List<Instance>();
        public readonly List<Instance> Gains = new List<Instance>();
        public readonly List<Instance> Plays = new List<Instance>();
    }
}