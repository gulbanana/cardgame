using System.Linq;
using System.Text.Json;
using Cardgame.API;
using Cardgame.Shared;

namespace Cardgame.Server
{
    internal static class AI
    {
        public static ClientCommand PlayTurn(GameModel state)
        {
            var myHand = state.Hands[state.ActivePlayer].Select(All.Cards.ByName);
            var treasures = myHand.OfType<ITreasureCard>();
            if (treasures.Any())
            {
                return new PlayAllTreasuresCommand { Seq = state.Seq };
            }
            else
            {
                var priorities = new[]{ "Province", "Gold", "Silver", "Copper" }.Select(All.Cards.ByName);
                foreach (var card in priorities)
                {
                    if (state.Supply[card.Name] > 0 && state.MoneyRemaining >= card.GetCost(state.GetModifiers()))
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