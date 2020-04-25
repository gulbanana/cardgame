using System.Linq;
using System.Text;

namespace Cardgame.Cards.Seaside
{
    public class PirateShipMat : MatBase
    {
        public override string Art => "pirate-ship";
        public override string Label => "Pirate Ship";

        public override string GetContents(string[] cards, bool isOwner)
        {
            if (cards == null || !cards.Any()) return null;
            
            var builder = new StringBuilder();
            builder.AppendLine("<lines>");
            for (var i = 0; i < cards.Length; i += 2)
            {                
                builder.AppendLine("<spans>");
                builder.AppendLine(All.Cards.ByName(cards[i]).Text);
                if (cards.Length > i+1)
                {
                    builder.AppendLine(All.Cards.ByName(cards[i+1]).Text);
                }
                builder.AppendLine("</spans>");                
            }
            builder.AppendLine("</lines>");

            return builder.ToString();
        }
    }
}