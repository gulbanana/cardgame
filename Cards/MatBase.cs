using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public virtual string GetContents(IReadOnlyList<Instance> cards, bool isOwnerOrSpectator)
        {
            if (cards == null || !cards.Any()) return null;
            
            var builder = new StringBuilder();
            
            builder.AppendLine("<spans>");
            foreach (var name in cards.Names().Distinct())
            {
                var number = cards.Names().Count(id => id == name);
                builder.AppendLine($"<card suffix=' x{number}'>{name}</card>");
            }
            builder.AppendLine("</spans>");

            return builder.ToString();
        }
    }
}