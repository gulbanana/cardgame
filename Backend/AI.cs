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
            else
            {
                var priorities = new[]{ "Province", "Gold", "Silver", "Copper" }.Select(Cards.All.ByName);
                foreach (var card in priorities)
                {
                    if (state.Supply[card.Name] > 0 && state.MoneyRemaining >= card.Cost)
                    {
                        return new BuyCardCommand { Seq = state.Seq, Id = card.Name };
                    }
                }
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