using System.Linq;
using System.Text;
using Cardgame.API;

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

        public virtual string GetContents(string[] cards, bool isOwnerOrSpectator)
        {
            if (cards == null || !cards.Any()) return null;
            
            var builder = new StringBuilder();
            
            builder.AppendLine("<spans>");
            foreach (var name in cards.Distinct())
            {
                var number = cards.Count(id => id == name);
                builder.AppendLine($"<card suffix=' x{number}'>{name}</card>");
            }
            builder.AppendLine("</spans>");

            return builder.ToString();
        }
    }
}