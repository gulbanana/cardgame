using System.Collections.Generic;
using System.Linq;
using Cardgame.Shared;

namespace Cardgame.Cards.Seaside
{
    public class NativeVillageMat : MatBase
    {
        public override string Art => "native-village";
        public override string Label => "Native Village";

        public override string GetContents(IReadOnlyList<Instance> cards, bool isOwnerOrSpectator)
        {
            if (isOwnerOrSpectator)
            {
                return base.GetContents(cards, true);
            }
            else
            {
                if (cards != null && cards.Any())
                {
                    return $"{cards.Count} {(cards.Count == 1 ? "card" : "cards")}.";
                }
                else
                {
                    return null;
                }
            }
        }
    }
}