using System.Linq;

namespace Cardgame
{
    internal static class AI
    {
        public static ClientCommand PlayTurn(GameModel state)
        {
            var myHand = state.Hands[state.ActivePlayer].Select(Cards.All.ByName);
            var treasures = myHand.OfType<Cards.TreasureCardModel>();
            if (treasures.Any())
            {
                return new PlayAllTreasuresCommand { Seq = state.Seq };
            }

            return new EndTurnCommand { Seq = state.Seq };
        }
    }
}