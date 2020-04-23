using System.Collections.Generic;
using Cardgame.API;
using Cardgame.Shared;

namespace Cardgame.Cards
{
    public abstract class MatBase : IMat
    {
        public string Name { get; }
        public virtual string Label { get; }
        public abstract string Art { get; }        

        public MatBase()
        {
            Name = this.GetType().Name;
        }

        public abstract string GetContents(IReadOnlyList<Instance> cards);
    }
}