using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cardgame.API;
using Cardgame.Shared;

namespace Cardgame.Cards.Base
{
    public class TrashMat : MatBase
    {
        public override string Art => "trash";

        public override string GetContents(IReadOnlyList<Instance> cards)
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