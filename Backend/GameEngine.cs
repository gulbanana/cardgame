using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cardgame
{
    internal class GameEngine
    {
        private readonly HashSet<string> bots;
        private TaskCompletionSource<string> inputTCS;
        private bool isDemo;
        private string demoNextActive;

        public readonly GameModel Model;
        public event Action ActionUpdated;

        public GameEngine()
        {
            bots = new HashSet<string>();

            Model = new GameModel();
            Model.EventLog = new List<string>();
            Model.ChatLog = new List<LogEntry>();        
            Model.Players = new string[0];
        }

        public void Execute(string username, ClientCommand command)
        {
            lock (this)
            {
                try
                {
                    ExecuteImpl(username, command);
                }
                catch (CommandException e)
                {
                    LogEvent($"<error>Error: {e.Message}</error>");
                }
            }
        }

        private void ExecuteImpl(string username, ClientCommand command)
        {
            if (Model.Seq != command.Seq)
            {
                throw new CommandException("Incorrect sequence number.");
            }

            switch (command)
            {
                case SetDemoCommand demo:
                    if (Model.Seq != 0) throw new CommandException("Game has been modified and cannot be set to demo mode.");

                    isDemo = true;

                    break;

                case SetNextPlayerCommand nextPlayer:
                    if (!isDemo) throw new CommandException("Game must be in demo mode.");

                    demoNextActive = nextPlayer.Player;

                    break;

                case ChatCommand chat:
                    if (chat.Message.Length > LogEntry.MAX) throw new CommandException("Chat message too long.");

                    Model.ChatLog.Add(new LogEntry { Username = username, Message = chat.Message });

                    break;

                case JoinGameCommand joinGame:
                    if (Model.IsStarted) throw new CommandException("The game is already in progress.");
                    if (Model.Players.Contains(username)) throw new CommandException("You are already in the game.");
                    if (Model.Players.Length >= 4) throw new CommandException("The game is full.");

                    Model.Players = Model.Players.Append(username).ToArray();
                    if (joinGame.IsBot)
                    {
                        bots.Add(username);
                    }

                    LogEvent($@"<spans>
                        <player>{username}</player>
                        <if you='join' them='joins'>{username}</if>
                        <run>the game.</run>
                    </spans>");
                    break;

                case LeaveGameCommand _:
                    if (Model.IsStarted) throw new CommandException("The game is already in progress.");
                    if (!Model.Players.Contains(username)) throw new CommandException("You are not in the game.");

                    Model.Players = Model.Players.Except(new[]{username}).ToArray();
                    bots.Remove(username);

                    LogEvent($@"<spans>
                        <player>{username}</player>
                        <if you='leave' them='leaves'>{username}</if>
                        <run>the game.</run>
                    </spans>");
                    break;

                case StartGameCommand startGame:
                    if (Model.IsStarted) throw new CommandException("The game is already in progress.");
                    if (!Model.Players.Contains(username)) throw new CommandException("You are not in the game.");
                    if (Model.Players.Length < 2) throw new CommandException("Not enough players.");

                    Model.KingdomSet = startGame.KingdomSet;
                    BeginGame();
                    BeginTurn();

                    break;

                case PlayCardCommand playCard:
                    if (Model.IsFinished) throw new CommandException("The game is over.");
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");
                    if (!Model.Hands[username].Contains(playCard.Id)) throw new CommandException($"You don't have a {playCard.Id} card in your hand.");
                    if (!Cards.All.Exists(playCard.Id)) throw new CommandException($"Card {playCard.Id} is not implemented.");

                    PlayCard(username, playCard.Id);

                    break;

                case PlayAllTreasuresCommand _:
                    if (Model.IsFinished) throw new CommandException("The game is over.");
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");

                    foreach (var card in Model.Hands[username].Select(Cards.All.ByName).OfType<Cards.TreasureCardModel>().ToList())
                    {
                        PlayCard(username, card.Name);
                    }

                    break;

                case BuyCardCommand buyCard:
                    if (Model.IsFinished) throw new CommandException("The game is over.");
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");
                    if (Model.BuysRemaining < 1) throw new CommandException("You have no remaining buys.");
                    if (Model.Stacks[buyCard.Id] < 1) throw new CommandException($"There are no {buyCard.Id} cards remaining.");

                    BuyCard(username, buyCard.Id);
                    
                    if (Model.BuysRemaining == 0)
                    {
                        EndTurn();
                        BeginTurn();
                    }
                    else
                    {
                        SkipBuyIfNoCash();
                    }

                    break;

                case EnterChoiceCommand choice:
                    if (Model.IsFinished) throw new CommandException("The game is over.");
                    if (Model.ChoosingPlayers.Peek() != username) throw new CommandException("You are not the choosing player.");

                    inputTCS.SetResult(choice.Output);

                    break;

                case EndTurnCommand _:
                    if (Model.IsFinished) throw new CommandException("The game is over.");
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");

                    EndTurn();
                    BeginTurn();
                    break;

                case var unknown:
                    throw new CommandException($"Unrecognised command {unknown}");
            }

            Model.Seq++;
        }

        private void BuyCard(string player, string id)
        {
            var boughtCard = Cards.All.ByName(id);
            if (boughtCard.Cost > Model.MoneyRemaining) throw new CommandException($"You don't have enough money to buy card {id}.");

            Model.Stacks[id]--;
            Model.Discards[player].Add(id);
            Model.MoneyRemaining -= boughtCard.Cost;
            Model.BuysRemaining -= 1;

            LogEvent($@"<spans>
                <player>{player}</player>
                <if you='buy' them='buys'>{player}</if>
                <card suffix='.'>{id}</card>
            </spans>");
        }

        private void PlayCard(string player, string id)
        {
            Model.Hands[player].Remove(id);
            Model.PlayedCards.Add(id);

            LogEvent($@"<spans>
                <player>{player}</player>
                <if you='play' them='plays'>{player}</if>
                <card suffix='.'>{id}</card>
            </spans>");

            var playedCard = Cards.All.ByName(id);                    
            switch (playedCard)
            {                    
                case Cards.ActionCardModel action:
                    if (Model.BuyPhase) throw new CommandException($"The Action phase is over.");
                    if (Model.ActionsRemaining < 1) throw new CommandException("You have no remaining actions.");

                    Model.ActionsRemaining--;
                    Model.IsExecutingAction = true;
                    var host = new ActionHost(this, Model.ActivePlayer);
                    action.ExecuteActionAsync(host).ContinueWith(CompleteAction);
                    break;
                
                case Cards.TreasureCardModel treasure:
                    if (!Model.BuyPhase)
                    {
                        Model.BuyPhase = true;
                        SkipBuyIfNoCash();
                    }
                    Model.MoneyRemaining += treasure.Value;
                    break;

                default:
                    throw new CommandException($"You can't play {playedCard.Type} cards.");
            }
        }

        private void CompleteAction(Task t)
        {
            lock (this)
            {
                if (t.Status == TaskStatus.Faulted)
                {
                    var e = t.Exception.InnerException ?? t.Exception;
                    LogEvent($"<error>Error: {e.Message}</error>");
                    // rollback somehow?
                }

                Model.IsExecutingAction = false;

                if (Model.ActionsRemaining == 0)
                {
                    Model.BuyPhase = true;
                    SkipBuyIfNoCash();
                }

                ActionUpdated?.Invoke();
            }
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

            // first, try to apply config
            Model.KingdomCards = Model.KingdomSet switch 
            {
                CardSet.FirstGame => new[]
                { "Cellar", "Market", "Militia", "Mine", "Moat", "Remodel", "Smithy", "Village", "Woodcutter", "Workshop" },

                CardSet.BigMoney => new[] 
                { "Adventurer", "Bureaucrat", "Chancellor", "Chapel", "Feast", "Laboratory", "Market", "Mine", "Moneylender", "Throne Room" },

                CardSet.Interaction => new[] 
                { "Bureaucrat", "Chancellor", "Council Room", "Festival", "Library", "Militia", "Moat", "Spy", "Thief", "Village" },

                CardSet.SizeDistortion => new[] 
                { "Cellar", "Chapel", "Feast", "Gardens", "Laboratory", "Thief", "Village", "Witch", "Woodcutter", "Workshop" },

                CardSet.VillageSquare => new[] 
                { "Bureaucrat", "Cellar", "Festival", "Library", "Market", "Remodel", "Smithy", "Throne Room", "Village", "Woodcutter" },

                _ => throw new CommandException($"Unknown card set {Model.KingdomSet}")
            };

            var byCost = Model.KingdomCards.Select(Cards.All.ByName).OrderBy(card => card.Cost).Select(card => card.Name).ToArray();
            Model.KingdomCards[0] = byCost[0];
            Model.KingdomCards[5] = byCost[1];
            Model.KingdomCards[1] = byCost[2];
            Model.KingdomCards[6] = byCost[3];
            Model.KingdomCards[2] = byCost[4];
            Model.KingdomCards[7] = byCost[5];
            Model.KingdomCards[3] = byCost[6];
            Model.KingdomCards[8] = byCost[7];
            Model.KingdomCards[4] = byCost[8];
            Model.KingdomCards[9] = byCost[9];

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

            var victoryCount = Model.Players.Length == 2 ? 8 : 12;
            Model.Stacks = new Dictionary<string, int>
            {
                { "Estate", victoryCount },
                { "Duchy", victoryCount },
                { "Province", victoryCount },
                { "Copper", 60 - (Model.Players.Length * 7) },
                { "Silver", 40 },
                { "Gold", 30 },
                { "Curse", (Model.Players.Length - 1) * 10 },
            };
            foreach (var card in Model.KingdomCards.Select(Cards.All.ByName))
            {
                Model.Stacks[card.Name] = card.Type == CardType.Victory ? victoryCount : 10;
            }

            foreach (var player in Model.Players)
            {
                if (isDemo)
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
            
            Model.ActivePlayer = isDemo ? demoNextActive : Model.Players[rng.Next(Model.Players.Length)];
        }

        private void BeginTurn()
        {
            if (Model.IsFinished) return;

            Model.ActionsRemaining = 1;
            Model.BuysRemaining = 1;
            Model.MoneyRemaining = isDemo ? 10 : 0;

            Model.BuyPhase = !Model.Hands[Model.ActivePlayer]
                .Select(Cards.All.ByName)
                .OfType<Cards.ActionCardModel>()
                .Any();

            LogEvent($@"<block>
                <spans>
                    <run>---</run>
                    <if you='Your' them=""{Model.ActivePlayer}'s"">{Model.ActivePlayer}</if>
                    <run>turn ---</run>
                </spans>
            </block>");

            if (!isDemo && bots.Contains(Model.ActivePlayer))
            {
                var botPlayer = Model.ActivePlayer;
                while (botPlayer == Model.ActivePlayer)
                {
                    var command = AI.PlayTurn(Model);
                    Execute(botPlayer, command);
                }
            }
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
                MoveCard(Model.ActivePlayer, hand[0], Zone.Hand, Zone.Discard);
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

            if (Model.Stacks["Province"] == 0 || Model.Stacks.Values.Where(v => v == 0).Count() >= 3)
            {
                EndGame();
            }
            else
            {
                if (!isDemo)
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
                    Model.ActivePlayer = demoNextActive;
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
                    var victoryCards = dominion.Select(Cards.All.ByName).OfType<Cards.VictoryCardModel>().GroupBy(card => card.Name);
                    var total = 0;
                    foreach (var group in victoryCards)
                    {
                        var exemplar = group.First();
                        var score = exemplar.VictoryPoints * group.Count();
                        total += score;
                        builder.AppendLine("<spans>");
                        builder.AppendLine($"<card>{group.Key}</card>");
                        builder.AppendLine($"<run>x{group.Count()}: {score} VP</run>");
                        builder.AppendLine("</spans>");
                    }
                    builder.AppendLine($"<run>Total: {total} Victory Points</run>");
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
                .Select(Cards.All.ByName)
                .OfType<Cards.TreasureCardModel>()
                .Select(card => card.Value)
                .Sum();

            var minimumCost = Model.Stacks
                .Where(kvp => kvp.Value > 0)
                .Select(kvp => Cards.All.ByName(kvp.Key).Cost)
                .Min();

            if (totalRemaining < minimumCost)
            {
                EndTurn();
                BeginTurn();
            }
        }

        internal bool DrawCard(string player, string id = null, Zone to = Zone.Hand)
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
                MoveCard(player, id, Zone.TopDeck, to);
            }
            else
            {
                var first = deck[0];
                MoveCard(player, first, Zone.TopDeck, to);
            }

            return reshuffled;
        }

        internal void MoveCard(string player, string id, Zone from, Zone to)
        {
            switch (from)
            {
                case Zone.Trash:
                    if (!Model.Trash.Contains(id)) throw new CommandException($"No {id} card in hand.");
                    Model.Trash.Remove(id);
                    break;

                case Zone.Hand:
                    if (!Model.Hands[player].Contains(id)) throw new CommandException($"No {id} card in hand.");
                    Model.Hands[player].Remove(id);
                    break;

                case Zone.Discard:
                    if (!Model.Discards[player].Contains(id)) throw new CommandException($"No {id} card in discard pile.");
                    Model.Discards[player].Remove(id);
                    break;
                
                case Zone.Stacks:
                    if (Model.Stacks[id] < 1) throw new CommandException($"No {id} cards remaining in stack.");
                    Model.Stacks[id]--;
                    break;

                case Zone.TopDeck:
                    if (!Model.Decks[player].First().Equals(id)) throw new CommandException($"Top of deck is not card {id}.");
                    Model.Decks[player].RemoveAt(0);
                    break;

                default:
                    throw new CommandException($"Unknown zone {from}");
            }

            switch (to)
            {
                case Zone.Trash:
                    Model.Trash.Add(id);
                    break;

                case Zone.Hand:
                    Model.Hands[player].Add(id);
                    break;

                case Zone.Discard:
                    Model.Discards[player].Add(id);
                    break;
                
                case Zone.Stacks:
                    Model.Stacks[id]++;
                    break;

                case Zone.TopDeck:
                    Model.Decks[player].Insert(0, id);
                    break;

                default:
                    throw new Exception($"Unknown zone {to}");
            }
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