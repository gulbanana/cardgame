using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

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
            Model.EventLog = new List<string>();
            Model.ChatLog = new List<LogEntry>();        
            Model.Players = new string[0];
        }

        private void LogEvent(string eventText)
        {
            Model.EventLog.Add(eventText);
        }

        internal void LogPartialEvent(string eventText)
        {
            var partial = Model.EventLog[Model.EventLog.Count - 1];
            var partialXML = XDocument.Parse(partial).Root;
            var finalXML = partialXML.Name == "lines" ? partialXML : new XElement("lines",
                partialXML
            );

            var eventXML = XDocument.Parse(eventText).Root;
            finalXML.Add(eventXML);
            
            Model.EventLog[Model.EventLog.Count - 1] = finalXML.ToString();
        }

        private void BeginGame()
        {
            var rng = new Random();

            Model.IsStarted = true;

            Model.PlayedCards = new List<string>();
            Model.Trash = new List<string>();
            Model.ChoosingPlayers = new Stack<string>();
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
                    DrawCard(player, "Copper");
                    DrawCard(player, "Copper");
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
            
            Model.ActivePlayer = Model.IsDemo ? Model.DemoNextActive : Model.Players[rng.Next(Model.Players.Length)];
        }

        private void BeginTurn()
        {
            if (Model.IsFinished) return;

            Model.ActionsRemaining = 1;
            Model.BuysRemaining = 1;
            Model.MoneyRemaining = Model.IsDemo ? 10 : 0;

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
                    <player prefix='('>{Model.ActivePlayer}</player>
                    <if you='reshuffle.)' them='reshuffles.)'>{Model.ActivePlayer}</if>
                </spans>");
            }

            if (Model.CardStacks["Province"] == 0 || Model.CardStacks.Values.Where(v => v == 0).Count() >= 3)
            {
                EndGame();
            }
            else
            {
                if (!Model.IsDemo)
                {
                    var nextPlayer = Array.FindIndex(Model.Players, e => e.Equals(Model.ActivePlayer)) + 1;
                    if (nextPlayer >= Model.Players.Length)
                    {
                        nextPlayer = 0;
                    }
                    Model.ActivePlayer = Model.Players[nextPlayer];
                }
                else
                {
                    Model.ActivePlayer = Model.DemoNextActive;
                }
            }
        }

        private void EndGame()
        {
            Model.IsFinished = true;

            LogEvent($@"<block>
                <spans>
                    <run>--- Game over ---</run>
                </spans>
            </block>");
            
            foreach (var text in Model.Players.Select(player => 
            {
                var builder = new System.Text.StringBuilder();
                builder.AppendLine("<lines>");
                    builder.AppendLine($"<spans><player>{player}</player><run>scored:</run></spans>");                    
                    var dominion = Model.Decks[player].Concat(Model.Hands[player]).Concat(Model.Discards[player]);
                    var victoryCards = dominion.Select(id => Cards.All.ByName[id]).OfType<Cards.VictoryCardModel>().GroupBy(card => card.Name);
                    foreach (var group in victoryCards)
                    {
                        builder.AppendLine("<spans>");
                        builder.AppendLine($"<card>{group.Key}</card>");
                        builder.AppendLine($"<run>x{group.Count()}</run>");                    
                        builder.AppendLine("</spans>");
                    }                    
                builder.AppendLine("</lines>");
                return builder.ToString();
            })) 
            {
                LogEvent(text);
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

        internal bool DrawCard(string player, string id = null)
        {
            var deck = Model.Decks[player];        
            var hand = Model.Hands[player];

            var reshuffled = false;
            if (!deck.Any())
            {
                var discard = Model.Discards[player];
                deck.AddRange(discard);
                deck.Shuffle();
                discard.Clear();
                reshuffled = true;
            }

            if (id != null)
            {
                deck.Remove(id);
                hand.Add(id);
            }
            else
            {
                var first = deck[0];
                deck.RemoveAt(0);
                hand.Add(first);
            }

            return reshuffled;
        }

        internal void DiscardCard(string player, string id)
        {
            Model.Hands[player].Remove(id);
            Model.Discards[player].Add(id);
        }

        internal void TrashCard(string player, string id)
        {
            Model.Hands[player].Remove(id);
            Model.Trash.Add(id);
        }

        internal void GainCard(string player, string id)
        {
            Model.CardStacks[id]--;
            Model.Discards[player].Add(id);
        }

        internal async Task<TOutput> Choose<TInput, TOutput>(string player, ChoiceType type, string prompt, TInput input)
        {
            Model.ChoosingPlayers.Push(player);
            Model.ChoiceType = type;
            Model.ChoicePrompt = prompt;
            Model.ChoiceInput = JsonSerializer.Serialize(input);

            inputTCS = new TaskCompletionSource<string>();
            ActionUpdated?.Invoke();
            var output = await inputTCS.Task;

            Model.ChoosingPlayers.Pop();

            return JsonSerializer.Deserialize<TOutput>(output);
        }
    }
}