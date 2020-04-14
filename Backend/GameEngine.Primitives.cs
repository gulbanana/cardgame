using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cardgame
{
    public partial class GameEngine
    {
        public readonly GameModel Model;
        public event Action ActionUpdated;
        private TaskCompletionSource<string> inputTCS;

        public GameEngine()
        {
            Model = new GameModel();
        }

        private void LogEvent(string eventText)
        {
            Model.EventLog.Add(TextModel.Parse(eventText));
        }

        private void LogPartialEvent(string eventText)
        {
            var partial = Model.EventLog[Model.EventLog.Count - 1];
            var final = partial is TextModel.Lines l ? l : new TextModel.Lines
            {
                Children = new[] { partial }
            };

            final.Children = final.Children.Append(TextModel.Parse(eventText)).ToArray();
            
            Model.EventLog[Model.EventLog.Count - 1] = final;
        }

        private void BeginGame()
        {
            var rng = new Random();

            Model.IsStarted = true;

            Model.PlayedCards = new List<string>();
            Model.Hands = Model.Players.ToDictionary(k => k, _ => new List<string>());
            Model.Discards = Model.Players.ToDictionary(k => k, _ => new List<string>());
            Model.Decks = Model.Players.ToDictionary(k => k, _ => 
            {
                var deck = new List<string>{ "Copper", "Copper", "Copper", "Copper", "Copper", "Copper", "Copper", "Estate", "Estate", "Estate" };
                deck.Shuffle();
                return deck;
            });

            Model.KingdomCards = new[] // XX pick at random instead!
            {
                "Cellar", "Market", "Mine", "Remodel", "Moat",
                "Smithy", "Village", "Woodcutter", "Workshop", "Militia"
            };

            var victoryCount = Model.Players.Length == 2 ? 8 : 12;
            Model.CardStacks = new Dictionary<string, int>
            {
                { "Estate", victoryCount },
                { "Duchy", victoryCount },
                { "Province", victoryCount },
                { "Copper", 60 - (Model.Players.Length * 7) },
                { "Silver", 40 },
                { "Gold", 30 },
                { "Curse", (Model.Players.Length - 1) * 10 },
            };
            foreach (var card in Model.KingdomCards.Select(id => Cards.All.ByName[id]))
            {
                Model.CardStacks[card.Name] = card.Type == CardType.Victory ? victoryCount : 10;
            }

            foreach (var player in Model.Players)
            {
                if (Model.IsDemo)
                {
                    DrawCard(player, "Copper");
                    DrawCard(player, "Copper");
                    DrawCard(player, "Copper");
                    DrawCard(player, "Estate");
                    DrawCard(player, "Estate");
                }
                else
                {
                    DrawCard(player);
                    DrawCard(player);
                    DrawCard(player);
                    DrawCard(player);
                    DrawCard(player);
                }
            }
            
            Model.ActivePlayer = Model.IsDemo ? Model.Players.Last() : Model.Players[rng.Next(Model.Players.Length)];
        }

        private void BeginTurn()
        {
            Model.ActionsRemaining = 1;
            Model.BuysRemaining = 1;
            Model.MoneyRemaining = 0;

            Model.BuyPhase = !Model.Hands[Model.ActivePlayer]
                .Select(id => Cards.All.ByName[id])
                .OfType<Cards.ActionCardModel>()
                .Any();

            LogEvent($@"<block>
                <spans>
                    <run>---</run>
                    <if you='Your' them=""{Model.ActivePlayer}'s"">{Model.ActivePlayer}</if>
                    <run>turn ---</run>
                </spans>
            </block>");
        }

        private void EndTurn()
        {
            var discard = Model.Discards[Model.ActivePlayer];
            while (Model.PlayedCards.Any())
            {
                var first = Model.PlayedCards[0];
                Model.PlayedCards.RemoveAt(0);
                discard.Add(first);
            }

            var hand = Model.Hands[Model.ActivePlayer];
            while (hand.Any())
            {
                DiscardCard(Model.ActivePlayer, hand[0]);
            }

            var reshuffled = false;
            for (var i = 0; i < 5; i++)
            {
                reshuffled = reshuffled | DrawCard(Model.ActivePlayer);
            }
            if (reshuffled)
            {
                LogEvent($@"<spans>
                    <run>(</run>
                    <player>{Model.ActivePlayer}</player>
                    <if you='reshuffle.' them='reshuffles.'>{Model.ActivePlayer}</if>
                    <run>)</run>
                </spans>");
            }

            if (!Model.IsDemo)
            {
                var nextPlayer = Array.FindIndex(Model.Players, e => e.Equals(Model.ActivePlayer)) + 1;
                if (nextPlayer >= Model.Players.Length)
                {
                    nextPlayer = 0;
                }
                Model.ActivePlayer = Model.Players[nextPlayer];
            }
        }

        private void SkipBuyIfNoCash()
        {
            var totalRemaining = Model.MoneyRemaining + Model.Hands[Model.ActivePlayer]
                .Select(id => Cards.All.ByName[id])
                .OfType<Cards.TreasureCardModel>()
                .Select(card => card.Value)
                .Sum();

            var minimumCost = Model.CardStacks
                .Where(kvp => kvp.Value > 0)
                .Select(kvp => Cards.All.ByName[kvp.Key].Cost)
                .Min();

            if (totalRemaining < minimumCost)
            {
                EndTurn();
                BeginTurn();
            }
        }

        private bool DrawCard(string player, string card = null)
        {
            var deck = Model.Decks[player];
            var hand = Model.Hands[player];

            if (card != null)
            {
                deck.Remove(card);
                hand.Add(card);
            }
            else
            {
                var first = deck[0];
                deck.RemoveAt(0);
                hand.Add(first);
            }
            
            if (!deck.Any())
            {
                var discard = Model.Discards[player];
                deck.AddRange(discard);
                deck.Shuffle();
                discard.Clear();

                return true;
            }
            else
            {
                return false;
            }
        }

        private void DiscardCard(string player, string card)
        {
            Model.Hands[player].Remove(card);
            Model.Discards[player].Add(card);
        }

        private async Task<TOutput> Choose<TInput, TOutput>(ChoiceType type, string prompt, TInput input)
        {
            Model.IsRequestingChoice = true;
            Model.ChoosingPlayer = Model.ActivePlayer;
            Model.ChoiceType = type;
            Model.ChoicePrompt = TextModel.Parse(prompt);
            Model.ChoiceInput = JsonSerializer.Serialize(input);

            inputTCS = new TaskCompletionSource<string>();
            var output = await inputTCS.Task;

            Model.IsRequestingChoice = false;

            return JsonSerializer.Deserialize<TOutput>(output);
        }
    }
}