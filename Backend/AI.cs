using System.Linq;
using System.Text.Json;

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

        public static string PlayChoice(GameModel state)
        {
            switch (state.ChoiceType)
            {                    
                case ChoiceType.SelectCards:
                    var input = JsonSerializer.Deserialize<SelectCards>(state.ChoiceInput);
                    var output = input.Choices.Take(input.Min??1).ToArray();
                    return JsonSerializer.Serialize<string[]>(output);

                case ChoiceType.YesNo:
                    return JsonSerializer.Serialize(true);

                default:
                    return null;
            }
        }
    }
}