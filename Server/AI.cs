using System.Linq;
using System.Text.Json;
using Cardgame.API;
using Cardgame.Choices;
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
                    if (state.Supply[card.Name] > 0 && state.MoneyRemaining >= card.GetCost(state))
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
                case ChoiceType.YesNo:
                    return JsonSerializer.Serialize(true);

                case ChoiceType.ChooseOne:
                    return JsonSerializer.Serialize(0);

                case ChoiceType.ChooseMultiple:
                    var cmInput = JsonSerializer.Deserialize<ChooseMultipleInput>(state.ChoiceInput);
                    var cmOutput = Enumerable.Range(0, cmInput.Number).ToArray();
                    return JsonSerializer.Serialize(cmOutput);

                case ChoiceType.SelectCards:
                    var scInput = JsonSerializer.Deserialize<SelectCardsInput>(state.ChoiceInput);
                    var scOutput = scInput.Choices.Take(scInput.Max??1).ToArray();
                    return JsonSerializer.Serialize<string[]>(scOutput);

                case ChoiceType.OrderCards:
                    return state.ChoiceInput;

                default:
                    return null;
            }
        }
    }
}