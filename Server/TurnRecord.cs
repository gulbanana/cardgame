using System.Collections.Generic;
using Cardgame.Shared;

namespace Cardgame.Server
{
    internal class TurnRecord
    {
        public readonly List<Instance> Gains = new List<Instance>();
    }
}