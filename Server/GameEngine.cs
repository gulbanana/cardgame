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
        private readonly Dictionary<string, TurnRecord> lastTurn;
        private TaskCompletionSource<string> inputTCS;
        private Instance? stash; // a temporary Zone used as cards move around during an action
        private int turn;

        internal readonly HashSet<Instance> IncompleteDurations;
        internal int ActionsThisTurn { get; private set; }

        public readonly GameModel Model;
        public event Action ActionUpdated;

        public GameEngine()
        {
            bots = new HashSet<string>();
            lastTurn = new Dictionary<string, TurnRecord>();
            IncompleteDurations = new HashSet<Instance>();

            Model = new GameModel();
            Model.EventLog = new List<string>();
            Model.ChatLog = new List<LogEntry>();        
            Model.Players = new string[0];
        }

        public bool Execute(string username, ClientCommand command)
        {
            lock (this)
            {
                try
                {
                    ExecuteImpl(username, command);
                    Notify();
                    return true;
                }
                catch (CommandException e)
                {
                    LogEvent($"<error>{username}: {e.Message}</error>");
                    Console.WriteLine(e.ToString());
                    Notify();
                    return false;
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
                    Model.KingdomCards = All.Presets.BySet(Model.KingdomSet)[Model.KingdomPreset];
                    Model.KingdomHasCurse = Model.KingdomCards.Any(All.Cards.UsesCurse);
                    Model.KingdomGlobalMats = new[] { "TrashMat" };
                    Model.KingdomPlayerMats = Model.KingdomCards.Select(All.Cards.ByName).SelectMany(card => card.HasMat != null ? new[] { card.HasMat } : new string[0]).ToArray();

                    break;

                case StartGameCommand startGame:
                    if (Model.IsStarted) throw new CommandException("The game is already in progress.");
                    if (!Model.Players.Contains(username)) throw new CommandException("You are not in the game.");
                    if (Model.Players.Length < 2) throw new CommandException("Not enough players.");

                    Model.KingdomSet = startGame.KingdomSet;
                    Model.KingdomPreset = startGame.KingdomPreset ?? All.Presets.BySet(Model.KingdomSet).Keys.First();
                    Model.KingdomCards = All.Presets.BySet(Model.KingdomSet)[Model.KingdomPreset];
                    Model.KingdomHasCurse = Model.KingdomCards.Any(All.Cards.UsesCurse);
                    Model.KingdomGlobalMats = new[] { "TrashMat" };
                    Model.KingdomPlayerMats = Model.KingdomCards.Select(All.Cards.ByName).SelectMany(card => card.HasMat != null ? new[] { card.HasMat } : new string[0]).ToArray();

                    BeginGame();
                    BeginBackgroundTask(Model.ActivePlayer, BeginTurnAsync);

                    break;

                case PlayCardCommand playCard:
                    if (!Model.IsStarted) throw new CommandException("The game has not begun.");
                    if (Model.IsFinished) throw new CommandException("The game is over.");
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");
                    if (Model.ExecutingBackgroundTasks) throw new CommandException("Another card is already being played.");
                    if (!Model.Hands[username].Contains(playCard.Id)) throw new CommandException($"You don't have a {playCard.Id} card in your hand.");
                    
                    if (!All.Cards.Exists(playCard.Id)) throw new CommandException($"Card {playCard.Id} is not implemented.");
                    var card = All.Cards.ByName(playCard.Id);

                    if (card.Types.Contains(CardType.Action))
                    {
                        if (Model.BuyPhase) throw new CommandException($"The Action phase is over.");
                        if (Model.ActionsRemaining < 1) throw new CommandException("You have no remaining actions.");
                        Model.ActionsRemaining--;
                    }
                    
                    if (card.Types.Contains(CardType.Treasure) && 
                        Model.SettingConfirmSkipActions[username] && 
                        !Model.BuyPhase &&
                        Model.ActionsRemaining > 0 &&
                        Model.Hands[username].Select(All.Cards.ByName).Any(card => card.Types.Contains(CardType.Action)))
                    {
                        BeginBackgroundTask(playCard.Id, async id =>
                        {
                            MoveCard(username, id, Zone.Hand, Zone.InPlay);
                            var skip = await Choose<string, bool>(
                                username, 
                                ChoiceType.YesNo, 
                                "Skip Action phase?",
                                "You have Action cards remaining. Do you really want to skip to the Treasure phase?"
                            );
                            MoveCard(username, id, Zone.InPlay, Zone.Hand);

                            if (skip)
                            {
                                Model.SettingConfirmSkipActions[username] = false;

                                LogEvent($@"<spans>
                                    <player>{username}</player>
                                    <if you='play' them='plays'>{username}</if>
                                    <card suffix='.'>{playCard.Id}</card>
                                </spans>");

                                await PlayCardAsync(1, username, id, Zone.Hand);
                            }
                        });
                    }
                    else
                    {
                        LogEvent($@"<spans>
                            <player>{username}</player>
                            <if you='play' them='plays'>{username}</if>
                            <card suffix='.'>{playCard.Id}</card>
                        </spans>");

                        BeginBackgroundTask(playCard.Id, id => PlayCardAsync(1, username, id, Zone.Hand));
                    }

                    break;

                case PlayAllTreasuresCommand _:
                    if (!Model.IsStarted) throw new CommandException("The game has not begun.");
                    if (Model.IsFinished) throw new CommandException("The game is over.");
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");
                    if (Model.ExecutingBackgroundTasks) throw new CommandException("Another card is already being played.");

                    var cards = Model.Hands[username].Select(All.Cards.ByName).OfType<ITreasureCard>().ToList();
                    var cardList = string.Join(Environment.NewLine, cards.Select((card, ix) => 
                    {
                        var suffix = ix == cards.Count - 1 ? "."
                            : ix < cards.Count - 2 ? ","
                            : " and";
                        return $"<card suffix='{suffix}'>{card.Name}</card>";
                    }));

                    if (Model.SettingConfirmSkipActions[username] && 
                        !Model.BuyPhase &&
                        Model.ActionsRemaining > 0 &&
                        Model.Hands[username].Select(All.Cards.ByName).Any(card => card.Types.Contains(CardType.Action)))
                    {
                        BeginBackgroundTask(username, async _ =>
                        {
                            var skip = await Choose<string, bool>(
                                username, 
                                ChoiceType.YesNo, 
                                "Skip Action phase?",
                                "You have Action cards remaining. Do you really want to skip to the Treasure phase?"
                            );

                            if (skip)
                            {
                                Model.SettingConfirmSkipActions[username] = false;

                                foreach (var card in cards)
                                {
                                    await PlayCardAsync(1, username, card.Name, Zone.Hand);
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
                            }
                        });
                    }
                    else
                    {
                        BeginBackgroundTask(username, async _ =>
                        {
                            foreach (var card in cards)
                            {
                                await PlayCardAsync(1, username, card.Name, Zone.Hand);
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
                        });
                    }

                    break;

                case BuyCardCommand buyCard:
                    if (!Model.IsStarted) throw new CommandException("The game has not begun.");
                    if (Model.IsFinished) throw new CommandException("The game is over.");
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");
                    if (Model.BuysRemaining < 1) throw new CommandException("You have no remaining buys.");
                    if (Model.Supply[buyCard.Id] < 1) throw new CommandException($"There are no {buyCard.Id} cards remaining.");
                    if (Model.ExecutingBackgroundTasks) throw new CommandException("A card is currently being played.");

                    BeginBackgroundTask(buyCard.Id, async id =>
                    {
                        await BuyCardAsync(username, id);
                        if (Model.BuysRemaining == 0)
                        {
                            await EndTurnAsync(Model.ActivePlayer);
                            await BeginTurnAsync(Model.ActivePlayer);
                        }
                    });

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
                    if (Model.ExecutingBackgroundTasks) throw new CommandException("A card is currently being played.");

                    BeginBackgroundTask(Model.ActivePlayer, async _ =>
                    {
                        await EndTurnAsync(Model.ActivePlayer);
                        await BeginTurnAsync(Model.ActivePlayer);
                    });
                    break;

                case var unknown:
                    throw new CommandException($"Unrecognised command {unknown}");
            }
        }

        private void Notify()
        {
            Model.Seq++;    
            ActionUpdated?.Invoke();
        }

        private void BeginBackgroundTask(string id, Func<string, Task> f)
        {
            if (Model.ExecutingBackgroundTasks)
            {
                throw new CommandException("A background task is already running.");
            }

            Model.ExecutingBackgroundTasks = true;
            try
            {
                var task = f(id);
                if (task.IsCompleted)
                {
                    CompleteBackgroundTask(task, id);
                }
                else
                {
                    task.ContinueWith(EndBackgroundTask, id);
                }
            }
            catch (CommandException)
            {
                Model.ExecutingBackgroundTasks = false;

                throw;
            }
            catch (Exception e)
            {
                Model.ExecutingBackgroundTasks = false;

                LogEvent($"<error>{id}: {e.Message}</error>");
                Console.WriteLine(e.ToString());
            }
        }

        private void EndBackgroundTask(Task t, object id)
        {
            lock (this)
            {
                CompleteBackgroundTask(t, id as string);
                Notify();
            }
        }

        private void CompleteBackgroundTask(Task t, string id)
        {
            Model.ExecutingBackgroundTasks = false;

            if (t.Status == TaskStatus.Faulted)
            {
                var e = t.Exception.InnerException ?? t.Exception;
                LogEvent($"<error>{id}: {e.Message}</error>");
                Console.WriteLine(e.ToString());
                // rollback somehow?
            }
        }

        private async Task BuyCardAsync(string player, string id)
        {
            var boughtCard = All.Cards.ByName(id);
            if (boughtCard.GetCost(Model) > Model.CoinsRemaining) throw new CommandException($"You don't have enough money to buy card {id}.");

            await Act(1, player, Trigger.BuyCard, id, () => 
            {
                Model.CoinsRemaining -= boughtCard.GetCost(Model);
                Model.BuysRemaining -= 1;
                
                var instance = MoveCard(player, id, Zone.SupplyAvailable, Zone.Discard);
                
                NoteBuy(player, instance);
                NoteGain(player, instance);

                LogEvent($@"<spans>
                    <player>{player}</player>
                    <if you='buy' them='buys'>{player}</if>
                    <card suffix='.'>{id}</card>
                </spans>");

                return Task.CompletedTask;
            }, Model.SupplyTokens[id].Select(All.Effects.ByName).OfType<IReactor>());
        }

        internal async Task PlayCardAsync(int indentLevel, string player, string id, Zone from)
        {       
            var played = MoveCard(player, id, from, Zone.InPlay);

            await Act(indentLevel, player, Trigger.PlayCard, id, async () =>
            {
                var card = All.Cards.ByName(id);
                if (card.Types.Contains(CardType.Duration))
                {
                    IncompleteDurations.Add(played);
                }

                if (card is IActionCard action)
                {
                    if (player == Model.ActivePlayer)
                    {
                        ActionsThisTurn++;
                    }

                    var host = new CardHost(this, indentLevel, player, played);
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

                    Model.CoinsRemaining += treasure.GetValue(Model);
                }
                else
                {
                    throw new CommandException($"Only Actions and Treasures can be played.");
                }
            }, Model.SupplyTokens[id].Select(All.Effects.ByName).OfType<IReactor>());
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

            Model.Supply = All.Cards.Base().Concat(Model.KingdomCards).ToDictionary(id => id, id => Model.GetInitialSupply(id));
            Model.SupplyTokens = Model.Supply.Keys.ToDictionary(k => k, _ => new string[0]);
            Model.ActiveEffects = new List<string>();
            Model.PreventedAttacks = new HashSet<string>();
            Model.ChoosingPlayers = new Stack<string>();
            Model.Hands = Model.Players.ToDictionary(k => k, _ => new List<Instance>());
            Model.Discards = Model.Players.ToDictionary(k => k, _ => new List<Instance>());
            Model.PlayedCards = Model.Players.ToDictionary(k => k, _ => new List<Instance>());
            Model.MatCards = Model.KingdomGlobalMats.ToDictionary(k => k, _ => new List<Instance>());
            Model.PlayerMatCards = Model.Players.ToDictionary(k => k, k => Model.KingdomPlayerMats.ToDictionary(k2 => k2, _ => new List<Instance>()));
            Model.Attachments = new Dictionary<Instance, Instance>();
            Model.Decks = Model.Players.ToDictionary(k => k, _ => 
            {
                var deck = new []{ "Copper", "Copper", "Copper", "Copper", "Copper", "Copper", "Copper", "Estate", "Estate", "Estate" }
                    .Select(Instance.Of)
                    .ToList();
                deck.Shuffle();
                return deck;
            });
            Model.SettingConfirmSkipActions = Model.Players.ToDictionary(k => k, _ => true);
            Model.SettingKeepHandSorted = Model.Players.ToDictionary(k => k, _ => true);
            Model.IsStarted = true;

            foreach (var player in Model.Players)
            {
                for (var i = 0; i < 5; i++)
                {
                    DrawCardIfAny(player);
                }
            }
            
            Model.ActivePlayer = Model.Players[rng.Next(Model.Players.Length)];
        }

        private async Task BeginTurnAsync(string player)
        {
            if (Model.IsFinished) return; // XXX what case does this cover?

            var turnNumber = (turn/Model.Players.Length)+1;
            lastTurn[player] = new TurnRecord(turnNumber);

            ActionsThisTurn = 0;
            Model.ActionsRemaining = 1;
            Model.BuysRemaining = 1;
            Model.CoinsRemaining = 0;
            Model.BuyPhase = false;
            Model.PreviouslyPlayedCards = new HashSet<Instance>(Model.PlayedCards[player]);

            LogEvent($@"<bold>
                <spans>
                    <run>---</run>
                    <if you='Your' them=""{player}'s"">{player}</if>
                    <run>turn {turnNumber} ---</run>
                </spans>
            </bold>");

            await Act(1, player, Trigger.BeginTurn, player, () => 
            {
                Model.BuyPhase = !Model.Hands[player]
                    .Select(All.Cards.ByName)
                    .OfType<IActionCard>()
                    .Any();

                return Task.CompletedTask;
            });

            if (bots.Contains(player))
            {
                var botPlayer = player;
                _ = Task.Run(() =>
                {
                    while (Model.ExecutingBackgroundTasks) {;}
                    var executed = true;
                    while (botPlayer == Model.ActivePlayer && !Model.IsFinished && executed)
                    {       
                        var command = AI.PlayTurn(Model);
                        executed = Execute(botPlayer, command);
                    }
                    if (!executed)
                    {
                        LogEvent($"<error>{botPlayer}: Sorry! I'm just a bot.</error>");
                    }
                });
            }
        }

        private async Task EndTurnAsync(string player)
        {
            var discard = Model.Discards[player];
            var inPlay = Model.PlayedCards[player];

            var toDiscard = new List<Instance>();
            foreach (var instance in inPlay.ToList())
            {
                var card = All.Cards.ByName(instance);
                if (!card.Types.Contains(CardType.Duration) || !IncompleteDurations.Contains(instance))
                {
                    var reactors = card is IReactor r ? new[]{r} : Array.Empty<IReactor>();
                    await Act(1, player, Trigger.DiscardCard, instance.Id, () =>
                    {
                        inPlay.Remove(instance);
                        discard.Add(instance);
                        return Task.CompletedTask;
                    }, reactors);
                }
            }

            Model.ActiveEffects.Clear();

            var hand = Model.Hands[player];
            while (hand.Any())
            {
                MoveCard(player, hand[0], Zone.Hand, Zone.Discard);
            }

            var reshuffled = false;
            for (var i = 0; i < 5; i++)
            {
                reshuffled = reshuffled | ReshuffleIfEmpty(player);
                DrawCardIfAny(player);
            }
            if (reshuffled)
            {
                LogEvent($@"<spans>
                    <player prefix='('>{player}</player>
                    <if you='reshuffle.)' them='reshuffles.)'>{player}</if>
                </spans>");
            }

            if (Model.Supply["Province"] == 0 || Model.Supply.Values.Where(v => v == 0).Count() >= 3)
            {
                EndGame();
            }
            else
            {
                var nextPlayer = Array.FindIndex(Model.Players, e => e.Equals(Model.ActivePlayer)) + 1;
                if (nextPlayer >= Model.Players.Length)
                {
                    nextPlayer = 0;
                }
                Model.ActivePlayer = Model.Players[nextPlayer];
                turn++;
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
            
            foreach (var scoreText in Model.Players.Select(player => All.Score.Calculate(Model, player).Text(lastTurn[player].TurnNumber)))
            {
                LogEvent(scoreText);
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

        internal string DrawCardIfAny(string player, string id = null, Zone? to = default)
        {
            var deck = Model.Decks[player];        
            var hand = Model.Hands[player];

            if (!deck.Any())
            {
                return null;
            }

            if (id != null)
            {
                MoveCard(player, id, Zone.DeckTop1, to ?? Zone.Hand);
            }
            else
            {
                id = deck[0].Id;
                MoveCard(player, id, Zone.DeckTop1, to ?? Zone.Hand);
            }

            return id;
        }

        internal Instance MoveCard(string player, string id, Zone from, Zone to)
        {
            Instance instance;

            switch (from.Name)
            {
                case ZoneName.Create:
                    instance = Instance.Of(id);
                    break;

                case ZoneName.DeckBottom:
                    if (Model.Decks[player].Last().Id != id) throw new CommandException($"No {id} card on bottom of deck.");
                    instance = Model.Decks[player].Last();
                    break;

                case ZoneName.DeckTop1:
                case ZoneName.DeckTop2:
                case ZoneName.DeckTop3:
                case ZoneName.DeckTop4:
                    instance = Model.Decks[player].Extract(id);
                    break;

                case ZoneName.Discard:
                    if (!Model.Discards[player].Contains(id)) throw new CommandException($"No {id} card in discard pile.");
                    instance = Model.Discards[player].Extract(id);
                    break;

                case ZoneName.Hand:
                    if (!Model.Hands[player].Contains(id)) throw new CommandException($"No {id} card in hand.");
                    instance = Model.Hands[player].Extract(id);
                    break;

                case ZoneName.InPlay:
                    if (!Model.PlayedCards[player].Contains(id)) throw new CommandException($"No {id} card is in play.");
                    instance = Model.PlayedCards[player].ExtractLast(id);
                    break;

                case ZoneName.PlayerMat:
                    var fromMat = (string)from.Param;
                    if (!Model.PlayerMatCards[player][fromMat].Contains(id)) throw new CommandException($"No {id} card on mat {fromMat}.");
                    instance = Model.PlayerMatCards[player][fromMat].ExtractLast(id);
                    break;

                case ZoneName.Stash:
                    if (!stash.HasValue || stash.Value.Id != id) throw new CommandException($"Stashed {id} not found.");
                    instance = stash.Value;
                    stash = null;
                    break;

                case ZoneName.Supply:
                    if (Model.Supply[id] < 1) throw new CommandException($"No {id} cards remaining in stack.");
                    Model.Supply[id]--;
                    instance = Instance.Of(id);
                    break;

                case ZoneName.Trash:
                    if (!Model.MatCards["TrashMat"].Contains(id)) throw new CommandException($"No {id} card in hand.");
                    instance = Model.MatCards["TrashMat"].Extract(id);
                    break;

                default:
                    throw new CommandException($"Unknown zone {from}");
            }

            switch (to.Name)
            {
                case ZoneName.DeckTop1:
                case ZoneName.DeckTop2:
                case ZoneName.DeckTop3:
                case ZoneName.DeckTop4:
                    Model.Decks[player].Insert(0, instance);
                    break;

                case ZoneName.Discard:
                    Model.Discards[player].Insert(0, instance);
                    break;

                case ZoneName.Hand:
                    Model.Hands[player].Add(instance);
                    break;

                case ZoneName.InPlay:
                    Model.PlayedCards[player].Add(instance);
                    break;

                case ZoneName.PlayerMat:
                    var toMat = (string)to.Param;
                    Model.PlayerMatCards[player][toMat].Add(instance);
                    break;

                case ZoneName.Stash:
                    if (stash.HasValue) throw new CommandException($"Card {stash} has been lost.");
                    stash = instance;
                    break;

                case ZoneName.Supply:
                    Model.Supply[instance.Id]++; // actual card is lost
                    break;

                case ZoneName.Trash:
                    Model.MatCards["TrashMat"].Add(instance);
                    break;

                default:
                    throw new Exception($"Unknown zone {to}");
            }

            return instance;
        }

        internal void MoveCard(string player, Instance instance, Zone from, Zone to)
        {
            MoveCard(player, instance.Id, from, to);
        }

        internal int CountCards(string player, Zone source)
        {
            return source.Name switch 
            {
                ZoneName.CountableDeck => Model.Decks[player].Count(),
                ZoneName.DeckTop1 => Model.Decks[player].Take(1).Count(),
                ZoneName.DeckTop2 => Model.Decks[player].Take(2).Count(),
                ZoneName.DeckTop3 => Model.Decks[player].Take(3).Count(),
                ZoneName.DeckTop4 => Model.Decks[player].Take(4).Count(),
                ZoneName.Discard => Model.Discards[player].Count(),
                ZoneName.Hand => Model.Hands[player].Count,
                ZoneName.InPlay => Model.PlayedCards.Count,
                ZoneName.PlayerMat => Model.PlayerMatCards[player][(string)source.Param].Count,
                ZoneName.Supply => Model.Supply.Keys.Count(id =>
                {
                    var supplyParam = (ValueTuple<bool, bool>)source.Param;
                    if (Model.Supply[id] > 0) 
                    {
                        return supplyParam.Item1;
                    }
                    else
                    {
                        return supplyParam.Item2;
                    }
                }),
                ZoneName.Stash => stash.HasValue ? 1 : 0,
                ZoneName.Trash => Model.MatCards["TrashMat"].Count,
                _ => throw new CommandException($"Unknown counting zone {source}")
            };
        }

        internal Instance[] GetInstances(string player, Zone source, Action onShuffle = null)
        {
            onShuffle = onShuffle ?? new Action(() => {;});

            if ((source == Zone.DeckTop1 || source == Zone.DeckBottom) && Model.Decks[player].Count < 1)
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

            return source.Name switch 
            {
                ZoneName.DeckBottom => new[] { Model.Decks[player].Last() },
                ZoneName.DeckTop1 => Model.Decks[player].Take(1).ToArray(),
                ZoneName.DeckTop2 => Model.Decks[player].Take(2).ToArray(),
                ZoneName.DeckTop3 => Model.Decks[player].Take(3).ToArray(),
                ZoneName.DeckTop4 => Model.Decks[player].Take(4).ToArray(),
                ZoneName.Discard => Model.Discards[player].ToArray(),
                ZoneName.Hand => Model.Hands[player].ToArray(),
                ZoneName.InPlay => Model.PlayedCards[player].ToArray(),
                ZoneName.PlayerMat => Model.PlayerMatCards[player][(string)source.Param].ToArray(),
                ZoneName.RecentBuys => lastTurn[(string)source.Param].Buys.ToArray(),
                ZoneName.RecentGains => lastTurn[(string)source.Param].Gains.ToArray(),
                ZoneName.Trash => Model.MatCards["TrashMat"].ToArray(),
                _ => throw new CommandException($"Unknown instance zone {source}")
            };
        }

        internal string[] GetCards(string player, Zone source, Action onShuffle = null)
        {
            return source.Name switch 
            {
                ZoneName.Supply => Model.Supply.Keys.Where(id => 
                {
                    var supplyParam = (ValueTuple<bool, bool>)source.Param;
                    if (Model.Supply[id] > 0) 
                    {
                        return supplyParam.Item1;
                    }
                    else
                    {
                        return supplyParam.Item2;
                    }
                }).ToArray(),
                _ => GetInstances(player, source, onShuffle).Names()
            };
        }

        internal void SetCardOrder(string player, string[] cards, Zone destination)
        {
            switch (destination.Name)
            {
                case ZoneName.DeckTop1:
                    break;

                case ZoneName.DeckTop2:
                    var found2 = new HashSet<Instance>();
                    var instance20 = Model.Decks[player].Take(2).First(i => i.Id == cards[0] && !found2.Contains(i)); found2.Add(instance20);
                    var instance21 = Model.Decks[player].Take(2).First(i => i.Id == cards[1] && !found2.Contains(i)); found2.Add(instance21);
                    Model.Decks[player][0] = instance20;
                    Model.Decks[player][1] = instance21;
                    break;

                case ZoneName.DeckTop3:
                    var found3 = new HashSet<Instance>();
                    var instance30 = Model.Decks[player].Take(3).First(i => i.Id == cards[0] && !found3.Contains(i)); found3.Add(instance30);
                    var instance31 = Model.Decks[player].Take(3).First(i => i.Id == cards[1] && !found3.Contains(i)); found3.Add(instance31);
                    var instance32 = Model.Decks[player].Take(3).First(i => i.Id == cards[2] && !found3.Contains(i)); found3.Add(instance32);
                    Model.Decks[player][0] = instance30;
                    Model.Decks[player][1] = instance31;
                    Model.Decks[player][2] = instance32;
                    break;

                case ZoneName.DeckTop4:
                    var found4 = new HashSet<Instance>();
                    var instance40 = Model.Decks[player].Take(4).First(i => i.Id == cards[0] && !found4.Contains(i)); found4.Add(instance40);
                    var instance41 = Model.Decks[player].Take(4).First(i => i.Id == cards[1] && !found4.Contains(i)); found4.Add(instance41);
                    var instance42 = Model.Decks[player].Take(4).First(i => i.Id == cards[2] && !found4.Contains(i)); found4.Add(instance42);
                    var instance43 = Model.Decks[player].Take(4).First(i => i.Id == cards[3] && !found4.Contains(i)); found4.Add(instance43);
                    Model.Decks[player][0] = instance40;
                    Model.Decks[player][1] = instance41;
                    Model.Decks[player][2] = instance42;
                    Model.Decks[player][3] = instance43;
                    break;

                case ZoneName.Discard:
                    var temp = Model.Discards[player].ToList();
                    Model.Discards[player].Clear();
                    foreach (var card in cards)
                    {
                        var instance = temp.First(i => i.Id == card);
                        temp.Remove(instance);
                        Model.Discards[player].Add(instance);
                    }
                    break;

                default:
                    throw new CommandException($"Unsupported Zone {destination} for reorder");
            }
        }
        
        internal void AttachStash(string player, Instance target)
        {
            Model.Attachments[target] = stash.Value;
            stash = null;
        }
        
        internal Instance DetachStash(string player, Instance target)
        {
            stash = Model.Attachments[target];
            Model.Attachments.Remove(target);
            return stash.Value;
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
            Notify();
            var output = await inputTCS.Task;

            Model.ChoosingPlayers.Pop();

            return JsonSerializer.Deserialize<TOutput>(output);
        }

        internal async Task Act(int indentLevel, string player, Trigger trigger, string parameter, Func<Task> f, IEnumerable<IReactor> extraReactors = null)
        {
            var reactions = new List<Reaction>();

            var globalHost = new TriggerHost(this, indentLevel, player, trigger, parameter);

            var globalReactors = Model.ActiveEffects
                .Select(All.Effects.ByName)
                .OfType<IReactor>()
                .ToList();            

            while (globalReactors.Any())
            {
                var reactor = globalReactors.First();
                globalReactors.Remove(reactor);                
                reactions.Add(await reactor.ExecuteReactionAsync(globalHost, Zone.InPlay, trigger, parameter));
            }

            var localReactors = extraReactors?.ToList();
            while (localReactors?.Any()??false)
            {
                var reactor = localReactors.First();
                localReactors.Remove(reactor);
                reactions.Add(await reactor.ExecuteReactionAsync(globalHost, Zone.This, trigger, parameter));
            }

            foreach (var owningPlayer in Model.Players)
            {
                var reacted = new HashSet<Instance>();
                var handCards = from instance in GetInstances(owningPlayer, Zone.Hand) select (instance: instance, zone: Zone.Hand);
                var playCards = from instance in GetInstances(owningPlayer, Zone.InPlay) select (instance: instance, zone: Zone.InPlay);
                var ownedReactors = handCards
                    .Concat(playCards)
                    .Where(t => !reacted.Contains(t.instance))
                    .Select(t => (t.instance, t.zone, card: All.Cards.ByName(t.instance)))
                    .Where(t => t.card is IReactor)
                    .Select(t => (t.instance, t.zone, reactor: (IReactor)t.card));

                while (ownedReactors.Any())
                {
                    var ownedReactor = ownedReactors.First();
                    reacted.Add(ownedReactor.instance);
                    var ownedHost = new CardHost(this, indentLevel, owningPlayer, ownedReactor.instance);
                    reactions.Add(await ownedReactor.reactor.ExecuteReactionAsync(ownedHost, ownedReactor.zone, trigger, parameter));
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

        internal void NoteBuy(string player, Instance instance)
        {
            if (player == Model.ActivePlayer)
            {
                lastTurn[player].Buys.Add(instance);
            }
        }

        internal void NoteGain(string player, Instance instance)
        {
            if (player == Model.ActivePlayer)
            {
                lastTurn[player].Gains.Add(instance);
            }
        }
    }
}