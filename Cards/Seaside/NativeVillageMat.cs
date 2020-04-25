using System.Linq;

namespace Cardgame.Cards.Seaside
{
    public class NativeVillageMat : MatBase
    {
        public override string Art => "native-village";
        public override string Label => "Native Village";

        public override string GetContents(string[] cards, bool isOwnerOrSpectator)
        {
            if (isOwnerOrSpectator)
            {
                return base.GetContents(cards, true);
            }
            else
            {
                if (cards != null && cards.Any())
                {
                    return $"{cards.Length} {(cards.Length == 1 ? "card" : "cards")}.";
                }
                else
                {
                    return null;
                }
            }
        }
    }
}