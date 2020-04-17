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
                    Model.KingdomPreset = startGame.KingdomPreset ?? All.Presets.BySet[Model.KingdomSet].Keys.First();

                    BeginGame();
                    BeginTurn();

                    break;

                case PlayCardCommand playCard:
                    if (Model.IsFinished) throw new CommandException("The game is over.");
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");
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
                    if (Model.IsFinished) throw new CommandException("The game is over.");
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");

                    foreach (var card in Model.Hands[username].Select(All.Cards.ByName).OfType<ITreasureCard>().ToList())
                    {
                        BeginPlayCard(1, username, card.Name, Zone.Hand);
                    }

                    break;

                case BuyCardCommand buyCard:
                    if (Model.IsFinished) throw new CommandException("The game is over.");
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");
                    if (Model.BuysRemaining < 1) throw new CommandException("You have no remaining buys.");
                    if (Model.Supply[buyCard.Id] < 1) throw new CommandException($"There are no {buyCard.Id} cards remaining.");

                    BuyCard(username, buyCard.Id);
                    
                    if (Model.BuysRemaining == 0)
                    {
                        EndTurn();
                        BeginTurn();
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
            var boughtCard = All.Cards.ByName(id);
            if (boughtCard.Cost > Model.MoneyRemaining) throw new CommandException($"You don't have enough money to buy card {id}.");

            Model.Supply[id]--;
            Model.Discards[player].Add(id);
            Model.MoneyRemaining -= boughtCard.Cost;
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
            }
        }

        private void EndPlayCard(Task t, object id)
        {
            lock (this)
            {
                CompletePlayCard(t, id as string);
                ActionUpdated?.Invoke();
            }
        }

        private void CompletePlayCard(Task t, string id)
        {
            if (t.Status == TaskStatus.Faulted)
            {
                var e = t.Exception.InnerException ?? t.Exception;
                LogEvent($"<error>{id}: {e.Message}</error>");
                // rollback somehow?
            }

            Model.ExecutingCards--;
        }

        internal async Task PlayCardAsync(int indentLevel, string player, string id, Zone from)
        {            
            await Act(indentLevel, player, Trigger.PlayCard, id, async () =>
            {
                MoveCard(player, id, from, Zone.InPlay);

                var card = All.Cards.ByName(id);
                switch (card)
                {                    
                    case IActionCard action:
                        var host = new ActionHost(indentLevel, this, player);
                        await action.ExecuteActionAsync(host);

                        if (Model.ActionsRemaining == 0)
                        {
                            Model.BuyPhase = true;
                        }

                        break;
                    
                    case ITreasureCard treasure:
                        if (!Model.BuyPhase)
                        {
                            Model.BuyPhase = true;
                        }

                        Model.MoneyRemaining += treasure.Value;

                        break;

                    default:
                        throw new CommandException($"You can't play {card.Type} cards.");
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

        private void BeginGame()
        {
            var rng = new Random();

            // first, try to apply config
            Model.KingdomCards = All.Presets.BySet[Model.KingdomSet][Model.KingdomPreset];
            var byCost = Model.KingdomCards.Select(All.Cards.ByName).OrderBy(card => card.Cost).Select(card => card.Name).ToArray();
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
            Model.ActiveEffects = new List<string>();
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
                Model.Supply[card.Name] = card.Type == CardType.Victory ? victoryCount : 10;
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

            Model.ActionsRemaining = 1;
            Model.BuysRemaining = 1;
            Model.MoneyRemaining = isDemo ? 10 : 0;

            Model.BuyPhase = !Model.Hands[Model.ActivePlayer]
                .Select(All.Cards.ByName)
                .OfType<IActionCard>()
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
                    var total = 0;
                    var dominion = Model.Decks[player].Concat(Model.Hands[player]).Concat(Model.Discards[player]).ToArray();
                    var victoryCards = dominion.Select(All.Cards.ByName).OfType<IVictoryCard>().GroupBy(card => card.Name);
                    foreach (var group in victoryCards)
                    {
                        var exemplar = group.First();
                        var score = exemplar.Score(dominion) * group.Count();
                        total += score;
                        builder.AppendLine("<spans>");
                        builder.AppendLine($"<card>{group.Key}</card>");
                        builder.AppendLine($"<run>x{group.Count()}: {score} VP</run>");
                        builder.AppendLine("</spans>");
                    }
                    var curseCards = dominion.Select(All.Cards.ByName).OfType<Cards.Base.Curse>();
                    if (curseCards.Any())
                    {
                        var score = curseCards.Count();
                        total -= score;
                        builder.AppendLine("<spans>");
                        builder.AppendLine($"<card>Curse</card>");
                        builder.AppendLine($"<run>x{curseCards.Count()}: -{score} VP</run>");
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
                
                case Zone.Supply:
                    if (Model.Supply[id] < 1) throw new CommandException($"No {id} cards remaining in stack.");
                    Model.Supply[id]--;
                    break;

                case Zone.DeckTop1:
                    if (!Model.Decks[player].First().Equals(id)) throw new CommandException($"Top of deck is not card {id}.");
                    Model.Decks[player].Remove(id);
                    break;

                case Zone.DeckTop2:
                    if (!Model.Decks[player].Take(2).Contains(id)) throw new CommandException($"Top of deck does not contain card {id}.");
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
                
                case Zone.Supply:
                    Model.Supply[id]++;
                    break;

                case Zone.DeckTop1:
                    Model.Decks[player].Insert(0, id);
                    break;

                case Zone.InPlay:
                    Model.PlayedCards.Add(id);
                    break;

                default:
                    throw new Exception($"Unknown zone {to}");
            }
        }

        internal string[] GetCards(string player, Zone source, Action onShuffle)
        {
            if (source == Zone.DeckTop1 && Model.Decks[player].Count < 1)
            {
                ReshuffleIfEmpty(player);
                onShuffle();
            }

            if (source == Zone.DeckTop2 && Model.Decks[player].Count < 2)
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
                Zone.Discard => Model.Discards[player].ToArray(),
                Zone.Hand => Model.Hands[player].ToArray(),
                Zone.InPlay => Model.PlayedCards.ToArray(),
                Zone.Supply => Model.KingdomCards.Concat(new[]{"Estate", "Duchy", "Province", "Copper", "Silver", "Gold", "Curse"}).Where(id => Model.Supply[id] > 0).ToArray(),
                Zone.Trash => Model.Trash.ToArray(),
                Zone other => throw new CommandException($"Unknown Zone {other}")
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

                case Zone.Discard:
                    Model.Discards[player] = cards.ToList();
                    break;

                default:
                    throw new CommandException($"Unsupported Zone {destination} for reorder");
            }
        }

        internal List<IReactor> GetReactions(string player)
        {
            return GetCards(player, Zone.Hand, () => {;})
                .Select(All.Cards.ByName) 
                .OfType<IReactor>()
                .Concat(Model.ActiveEffects.Select(All.Effects.ByName).OfType<IReactor>())
                .ToList();
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
            ActionUpdated?.Invoke();
            var output = await inputTCS.Task;

            Model.ChoosingPlayers.Pop();

            return JsonSerializer.Deserialize<TOutput>(output);
        }

        internal async Task Act(int indentLevel, string player, Trigger trigger, string parameter, Func<Task> f)
        {
            var reactions = GetReactions(player);
            var replaced = false;
            var afterReactions = new List<Action>();

            while (reactions.Any())
            {
                var potentialReaction = reactions.First();
                reactions.Remove(potentialReaction);

                var host = new ActionHost(indentLevel, this, player);
                var reaction = await potentialReaction.ExecuteReactionAsync(host, trigger, parameter);                
                switch (reaction.Type)
                {
                    case ReactionType.None:
                        break;

                    case ReactionType.Replace:
                    case ReactionType.Before:
                        replaced = reaction.Type == ReactionType.Replace;
                        reaction.Enact();
                        break;

                    case ReactionType.After:
                        afterReactions.Add(reaction.Enact);
                        break;

                    default:
                        throw new Exception($"unknown ReactionType {reaction.Type}");
                }
            }

            if (!replaced)
            {
                await f();
            }

            foreach (var enactor in afterReactions)
            {
                enactor();
            }
        }
    }
}