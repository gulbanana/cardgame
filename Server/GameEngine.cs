using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Cardgame.API;
using Cardgame.Shared;

namespace Cardgame.Server
{
    internal class GameEngine
    {
        private readonly HashSet<string> bots;
        private TaskCompletionSource<string> inputTCS;
        private bool isDemo;
        private string demoNextActive;
        internal int ActionsThisTurn { get; private set; }

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
                    LogEvent($"<error>{username}: {e.Message}</error>");
                    Console.WriteLine(e.ToString());
                }

                Model.Seq++; ActionUpdated?.Invoke();
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

                case ConfigureGameCommand configureGame:
                    if (Model.IsStarted) throw new CommandException("The game is already in progress.");

                    Model.KingdomSet = configureGame.KingdomSet;
                    Model.KingdomPreset = configureGame.KingdomPreset ?? All.Presets.BySet(Model.KingdomSet).Keys.First();

                    break;

                case StartGameCommand startGame:
                    if (Model.IsStarted) throw new CommandException("The game is already in progress.");
                    if (!Model.Players.Contains(username)) throw new CommandException("You are not in the game.");
                    if (Model.Players.Length < 2) throw new CommandException("Not enough players.");

                    Model.KingdomSet = startGame.KingdomSet;
                    Model.KingdomPreset = startGame.KingdomPreset ?? All.Presets.BySet(Model.KingdomSet).Keys.First();

                    BeginGame();
                    BeginTurn();

                    break;

                case PlayCardCommand playCard:
                    if (!Model.IsStarted) throw new CommandException("The game has not begun.");
                    if (Model.IsFinished) throw new CommandException("The game is over.");
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");
                    if (Model.ExecutingCards > 0) throw new CommandException("Another card is already being played.");
                    if (!Model.Hands[username].Contains(playCard.Id)) throw new CommandException($"You don't have a {playCard.Id} card in your hand.");
                    if (!All.Cards.Exists(playCard.Id)) throw new CommandException($"Card {playCard.Id} is not implemented.");

                    if (All.Cards.ByName(playCard.Id) is IActionCard)
                    {
                        if (Model.BuyPhase) throw new CommandException($"The Action phase is over.");
                        if (Model.ActionsRemaining < 1) throw new CommandException("You have no remaining actions.");
                        Model.ActionsRemaining--;
                    }

                    LogEvent($@"<spans>
                        <player>{username}</player>
                        <if you='play' them='plays'>{username}</if>
                        <card suffix='.'>{playCard.Id}</card>
                    </spans>");

                    BeginPlayCard(1, username, playCard.Id, Zone.Hand);

                    break;

                case PlayAllTreasuresCommand _:
                    if (!Model.IsStarted) throw new CommandException("The game has not begun.");
                    if (Model.IsFinished) throw new CommandException("The game is over.");
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");
                    if (Model.ExecutingCards > 0) throw new CommandException("Another card is already being played.");

                    var cards = Model.Hands[username].Select(All.Cards.ByName).OfType<ITreasureCard>().ToList();
                    var cardList = string.Join(Environment.NewLine, cards.Select((card, ix) => 
                    {
                        var suffix = ix == cards.Count - 1 ? "."
                            : ix < cards.Count - 2 ? ","
                            : " and";
                        return $"<card suffix='{suffix}'>{card.Name}</card>";
                    }));

                    foreach (var card in cards)
                    {
                        BeginPlayCard(1, username, card.Name, Zone.Hand);
                    }

                    var sum = cards.Select(card => card.GetValue(Model)).Sum();
                    LogEvent($@"<lines>
                        <spans>
                            <player>{username}</player>
                            <if you='play' them='plays'>{username}</if>
                            {cardList}
                        </spans>
                        <spans>
                            <indent level='1' />
                            {LogVerbInitial(username, "get", "gets", "getting")}
                            <run>+${sum}.</run>
                        </spans>
                    </lines>");

                    break;

                case BuyCardCommand buyCard:
                    if (!Model.IsStarted) throw new CommandException("The game has not begun.");
                    if (Model.IsFinished) throw new CommandException("The game is over.");
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");
                    if (Model.BuysRemaining < 1) throw new CommandException("You have no remaining buys.");
                    if (Model.Supply[buyCard.Id] < 1) throw new CommandException($"There are no {buyCard.Id} cards remaining.");
                    if (Model.ExecutingCards > 0) throw new CommandException("A card is currently being played.");

                    BuyCard(username, buyCard.Id);
                    
                    if (Model.BuysRemaining == 0)
                    {
                        EndTurn();
                        BeginTurn();
                    }

                    break;

                case EnterChoiceCommand choice:
                    if (!Model.IsStarted) throw new CommandException("The game has not begun.");
                    if (Model.IsFinished) throw new CommandException("The game is over.");
                    if (Model.ChoosingPlayers.Peek() != username) throw new CommandException("You are not the choosing player.");

                    inputTCS.SetResult(choice.Output);

                    break;

                case EndTurnCommand _:
                    if (!Model.IsStarted) throw new CommandException("The game has not begun.");
                    if (Model.IsFinished) throw new CommandException("The game is over.");
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");
                    if (Model.ExecutingCards > 0) throw new CommandException("A card is currently being played.");

                    EndTurn();
                    BeginTurn();
                    break;

                case var unknown:
                    throw new CommandException($"Unrecognised command {unknown}");
            }
        }

        private void BuyCard(string player, string id)
        {
            var boughtCard = All.Cards.ByName(id);
            if (boughtCard.GetCost(Model) > Model.MoneyRemaining) throw new CommandException($"You don't have enough money to buy card {id}.");

            Model.Supply[id]--;
            Model.Discards[player].Add(id);
            Model.MoneyRemaining -= boughtCard.GetCost(Model);
            Model.BuysRemaining -= 1;

            LogEvent($@"<spans>
                <player>{player}</player>
                <if you='buy' them='buys'>{player}</if>
                <card suffix='.'>{id}</card>
            </spans>");
        }

        private void BeginPlayCard(int indentLevel, string player, string id, Zone from)
        {
            Model.ExecutingCards++;
            try
            {
                var task = PlayCardAsync(indentLevel, player, id, from);
                if (task.IsCompleted)
                {
                    CompletePlayCard(task, id);
                }
                else
                {
                    task.ContinueWith(EndPlayCard, id);
                }
            }
            catch (CommandException)
            {
                Model.ExecutingCards--;

                throw;
            }
            catch (Exception e)
            {
                Model.ExecutingCards--;

                LogEvent($"<error>{id}: {e.Message}</error>");
                Console.WriteLine(e.ToString());
            }
        }

        private void EndPlayCard(Task t, object id)
        {
            CompletePlayCard(t, id as string);
            Model.Seq++; ActionUpdated?.Invoke();
        }

        private void CompletePlayCard(Task t, string id)
        {
            if (t.Status == TaskStatus.Faulted)
            {
                var e = t.Exception.InnerException ?? t.Exception;
                LogEvent($"<error>{id}: {e.Message}</error>");
                Console.WriteLine(e.ToString());
                // rollback somehow?
            }

            Model.ExecutingCards--;
        }

        internal async Task PlayCardAsync(int indentLevel, string player, string id, Zone from)
        {       
            MoveCard(player, id, from, Zone.InPlay);

            await Act(indentLevel, player, Trigger.PlayCard, id, async () =>
            {
                var card = All.Cards.ByName(id);
                if (card is IActionCard action)
                {
                    if (player == Model.ActivePlayer)
                    {
                        ActionsThisTurn++;
                    }

                    var host = new ActionHost(indentLevel, this, player);
                    await action.ExecuteActionAsync(host);

                    if (Model.ActionsRemaining == 0)
                    {
                        Model.BuyPhase = true;
                    }
                }
                else if (card is ITreasureCard treasure)
                {
                    if (!Model.BuyPhase)
                    {
                        Model.BuyPhase = true;
                    }

                    Model.MoneyRemaining += treasure.GetValue(Model);
                }
                else
                {
                    throw new CommandException($"Only Actions and Treasures can be played.");
                }
            });
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

        internal string LogVerbInitial(string player, string secondPerson, string thirdPerson, string continuous)
        {
            if (player == Model.ActivePlayer)
            {
                return $"<if you='you {secondPerson}' them='{continuous}'>{player}</if>";
            }
            else
            {
                return $"<player>{player}</player><if you='{secondPerson}' them='{thirdPerson}'>{player}</if>";
            }
        }

        private void BeginGame()
        {
            var rng = new Random();

            Model.KingdomCards = All.Presets.BySet(Model.KingdomSet)[Model.KingdomPreset];
            Model.IsStarted = true;
            Model.PlayedCards = new List<string>();
            Model.ActiveEffects = new List<string>();
            Model.Trash = new List<string>();
            Model.PreventedAttacks = new HashSet<string>();
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
            Model.Supply = new Dictionary<string, int>
            {
                { "Estate", victoryCount },
                { "Duchy", victoryCount },
                { "Province", victoryCount },
                { "Copper", 60 - (Model.Players.Length * 7) },
                { "Silver", 40 },
                { "Gold", 30 },
                { "Curse", (Model.Players.Length - 1) * 10 },
            };
            foreach (var card in Model.KingdomCards.Select(All.Cards.ByName))
            {
                Model.Supply[card.Name] = card.Types.Contains(CardType.Victory) ? victoryCount : 10;
            }

            foreach (var player in Model.Players)
            {
                for (var i = 0; i < 5; i++)
                {
                    if (isDemo)
                    {
                        DrawCardIfAny(player, "Copper");
                    }
                    else
                    {
                        DrawCardIfAny(player);
                    }
                }
            }
            
            Model.ActivePlayer = isDemo ? demoNextActive : Model.Players[rng.Next(Model.Players.Length)];
        }

        private void BeginTurn()
        {
            if (Model.IsFinished) return;

            ActionsThisTurn = 0;
            Model.ActionsRemaining = 1;
            Model.BuysRemaining = 1;
            Model.MoneyRemaining = isDemo ? 10 : 0;

            Model.BuyPhase = !Model.Hands[Model.ActivePlayer]
                .Select(All.Cards.ByName)
                .OfType<IActionCard>()
                .Any();

            LogEvent($@"<bold>
                <spans>
                    <run>---</run>
                    <if you='Your' them=""{Model.ActivePlayer}'s"">{Model.ActivePlayer}</if>
                    <run>turn ---</run>
                </spans>
            </bold>");

            if (!isDemo && bots.Contains(Model.ActivePlayer))
            {
                var botPlayer = Model.ActivePlayer;
                while (botPlayer == Model.ActivePlayer && !Model.IsFinished)
                {                    
                    try
                    {
                        var command = AI.PlayTurn(Model);
                        ExecuteImpl(botPlayer, command);
                    }
                    catch (Exception e)
                    {
                        LogEvent($"<error>{Model.ActivePlayer}: {e.Message}</error>");
                        Console.WriteLine(e.ToString());

                        EndTurn();
                        BeginTurn();
                        break;
                    }
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

            Model.ActiveEffects.Clear();

            var hand = Model.Hands[Model.ActivePlayer];
            while (hand.Any())
            {
                MoveCard(Model.ActivePlayer, hand[0], Zone.Hand, Zone.Discard);
            }

            var reshuffled = false;
            for (var i = 0; i < 5; i++)
            {
                reshuffled = reshuffled | ReshuffleIfEmpty(Model.ActivePlayer);
                DrawCardIfAny(Model.ActivePlayer);
            }
            if (reshuffled)
            {
                LogEvent($@"<spans>
                    <player prefix='('>{Model.ActivePlayer}</player>
                    <if you='reshuffle.)' them='reshuffles.)'>{Model.ActivePlayer}</if>
                </spans>");
            }

            if (Model.Supply["Province"] == 0 || Model.Supply.Values.Where(v => v == 0).Count() >= 3)
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

            LogEvent($@"<bold>
                <spans>
                    <run>--- Game over ---</run>
                </spans>
            </bold>");
            
            foreach (var score in Model.Players.Select(player => All.Score.Calculate(Model, player)))
            {
                LogEvent(score.Text());
            }
        }

        internal bool ReshuffleIfEmpty(string player)
        {
            var deck = Model.Decks[player];        

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

        internal string DrawCardIfAny(string player, string id = null, Zone to = Zone.Hand)
        {
            var deck = Model.Decks[player];        
            var hand = Model.Hands[player];

            if (!deck.Any())
            {
                return null;
            }

            if (id != null)
            {
                MoveCard(player, id, Zone.DeckTop1, to);
            }
            else
            {
                id = deck[0];
                MoveCard(player, id, Zone.DeckTop1, to);
            }

            return id;
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
                
                case Zone.SupplyAvailable:
                    if (Model.Supply[id] < 1) throw new CommandException($"No {id} cards remaining in stack.");
                    Model.Supply[id]--;
                    break;

                case Zone.DeckTop1:
                case Zone.DeckTop2:
                case Zone.DeckTop3:
                case Zone.DeckTop4:
                    Model.Decks[player].Remove(id);
                    break;

                case Zone.InPlay:
                    if (!Model.PlayedCards.Contains(id)) throw new CommandException($"No {id} card has been played.");
                    var last = Model.PlayedCards.FindLastIndex(e => e.Equals(id));
                    Model.PlayedCards.RemoveAt(last);
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
                    Model.Discards[player].Insert(0, id);
                    break;
                
                case Zone.SupplyAvailable:
                    Model.Supply[id]++;
                    break;

                case Zone.DeckTop1:
                case Zone.DeckTop2:
                case Zone.DeckTop3:
                case Zone.DeckTop4:
                    Model.Decks[player].Insert(0, id);
                    break;

                case Zone.InPlay:
                    Model.PlayedCards.Add(id);
                    break;

                default:
                    throw new Exception($"Unknown zone {to}");
            }
        }

        internal int CountCards(string player, Zone source)
        {
            return source switch 
            {
                Zone.CountableDeck => Model.Decks[player].Count(),
                Zone.DeckTop1 => Model.Decks[player].Take(1).Count(),
                Zone.DeckTop2 => Model.Decks[player].Take(2).Count(),
                Zone.DeckTop3 => Model.Decks[player].Take(3).Count(),
                Zone.DeckTop4 => Model.Decks[player].Take(4).Count(),
                Zone.Discard => Model.Discards[player].Count(),
                Zone.Hand => Model.Hands[player].Count,
                Zone.InPlay => Model.PlayedCards.Count,
                Zone.SupplyAll => Model.Supply.Keys.Count,
                Zone.SupplyAvailable => Model.Supply.Keys.Count(id => Model.Supply[id] > 0),
                Zone.SupplyEmpty => Model.Supply.Keys.Count(id => Model.Supply[id] == 0),
                Zone.Trash => Model.Trash.Count,
                Zone other => throw new CommandException($"Unknown card zone {other}")
            };
        }

        internal string[] GetCards(string player, Zone source, Action onShuffle)
        {
            if (source == Zone.DeckTop1 && Model.Decks[player].Count < 1)
            {
                ReshuffleIfEmpty(player);
                onShuffle();
            }
            else if (source == Zone.DeckTop2 && Model.Decks[player].Count < 2)
            {
                var setAside = Model.Decks[player].ToArray();
                Model.Decks[player].Clear();                
                ReshuffleIfEmpty(player);
                Model.Decks[player].InsertRange(0, setAside);
                onShuffle();
            }
            else if (source == Zone.DeckTop3 && Model.Decks[player].Count < 3)
            {
                var setAside = Model.Decks[player].ToArray();
                Model.Decks[player].Clear();                
                ReshuffleIfEmpty(player);
                Model.Decks[player].InsertRange(0, setAside);
                onShuffle();
            }
            else if (source == Zone.DeckTop4 && Model.Decks[player].Count < 4)
            {
                var setAside = Model.Decks[player].ToArray();
                Model.Decks[player].Clear();                
                ReshuffleIfEmpty(player);
                Model.Decks[player].InsertRange(0, setAside);
                onShuffle();
            }

            return source switch 
            {
                Zone.DeckTop1 => Model.Decks[player].Take(1).ToArray(),
                Zone.DeckTop2 => Model.Decks[player].Take(2).ToArray(),
                Zone.DeckTop3 => Model.Decks[player].Take(3).ToArray(),
                Zone.DeckTop4 => Model.Decks[player].Take(4).ToArray(),
                Zone.Discard => Model.Discards[player].ToArray(),
                Zone.Hand => Model.Hands[player].ToArray(),
                Zone.InPlay => Model.PlayedCards.ToArray(),
                Zone.SupplyAvailable => Model.Supply.Keys.Where(id => Model.Supply[id] > 0).ToArray(),
                Zone.SupplyEmpty => Model.Supply.Keys.Where(id => Model.Supply[id] == 0).ToArray(),
                Zone.SupplyAll => Model.Supply.Keys.ToArray(),
                Zone.Trash => Model.Trash.ToArray(),
                Zone other => throw new CommandException($"Unknown card zone {other}")
            };
        }

        internal void SetCards(string player, string[] cards, Zone destination)
        {
            switch (destination)
            {
                case Zone.DeckTop1:
                    Model.Decks[player][0] = cards[0];
                    break;

                case Zone.DeckTop2:
                    Model.Decks[player][0] = cards[0];
                    Model.Decks[player][1] = cards[1];
                    break;

                case Zone.DeckTop3:
                    Model.Decks[player][0] = cards[0];
                    Model.Decks[player][1] = cards[1];
                    break;

                case Zone.DeckTop4:
                    Model.Decks[player][0] = cards[0];
                    Model.Decks[player][1] = cards[1];
                    break;

                case Zone.Discard:
                    Model.Discards[player] = cards.ToList();
                    break;

                default:
                    throw new CommandException($"Unsupported Zone {destination} for reorder");
            }
        }

        internal async Task<TOutput> Choose<TInput, TOutput>(string player, ChoiceType type, string prompt, TInput input)
        {            
            Model.ChoiceType = type;
            Model.ChoicePrompt = prompt;
            Model.ChoiceInput = JsonSerializer.Serialize(input);

            if (bots.Contains(player))
            {
                var botOutput = AI.PlayChoice(Model);
                if (botOutput != null)
                {
                    return JsonSerializer.Deserialize<TOutput>(botOutput);
                }
            }

            Model.ChoosingPlayers.Push(player);           
             
            inputTCS = new TaskCompletionSource<string>();            
            Model.Seq++; ActionUpdated?.Invoke();
            var output = await inputTCS.Task;

            Model.ChoosingPlayers.Pop();

            return JsonSerializer.Deserialize<TOutput>(output);
        }

        internal async Task Act(int indentLevel, string player, Trigger trigger, string parameter, Func<Task> f)
        {
            var reactions = new List<Reaction>();

            var globalReactors = Model.ActiveEffects
                .Select(All.Effects.ByName)
                .OfType<IReactor>()
                .ToList();

            while (globalReactors.Any())
            {
                var reactor = globalReactors.First();
                globalReactors.Remove(reactor);
                var host = new ActionHost(indentLevel, this, player);
                reactions.Add(await reactor.ExecuteReactionAsync(host, trigger, parameter));
            }

            foreach (var target in Model.Players)
            {
                var targetReactors = GetCards(target, Zone.Hand, () => {;})
                    .Select(All.Cards.ByName) 
                    .OfType<IReactor>()
                    .ToList();

                while (targetReactors.Any())
                {
                    var reactor = targetReactors.First();
                    targetReactors.Remove(reactor);
                    var host = new ActionHost(indentLevel, this, target);
                    reactions.Add(await reactor.ExecuteReactionAsync(host, trigger, parameter));
                }
            }

            foreach (var reaction in reactions)
            {
                await reaction.ActBefore();
            }
            
            await f();

            foreach (var reaction in reactions)
            {
                await reaction.ActAfter();
            }
        }
    }
}