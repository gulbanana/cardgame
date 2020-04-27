using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Cardgame.All;
using Cardgame.API;
using Cardgame.Engine.Logging;
using Cardgame.Model;

[assembly:InternalsVisibleTo("Cardgame.Tests")]
namespace Cardgame.Engine
{
    public class GameEngine
    {
        private readonly HashSet<string> bots;
        private readonly List<Record> logs;
        private readonly Dictionary<string, TurnRecord> turns;
        private readonly Dictionary<string, int> turnNumbers;  
        internal readonly Queue<string> ExtraTurns;

        private TaskCompletionSource<string> inputTCS;
        private TaskCompletionSource<bool> backgroundTCS;
        public Task Background => backgroundTCS?.Task ?? Task.CompletedTask;

        // temporary zones, can only exist during an action
        private readonly Dictionary<int, Instance> stashes;
        private readonly List<Instance> revealed;

        // XXX replace with something in TurnRecord
        internal int ActionsThisTurn { get; private set; }

        // public state - each command results in 1+ model updates
        public readonly GameModel Model;
        public event Action ModelUpdated;
        private readonly LogManager logManager;

        public GameEngine()
        {
            bots = new HashSet<string>();
            logs = new List<Record>();
            turns = new Dictionary<string, TurnRecord>();
            turnNumbers = new Dictionary<string, int>();
            ExtraTurns = new Queue<string>();

            stashes = new Dictionary<int, Instance>();
            revealed = new List<Instance>(); // could move to the model, or have a copy "RecentlyRevealed"

            Model = new GameModel();
            Model.EventLog = new List<string>();
            Model.ChatLog = new List<LogEntry>();        
            Model.Players = new string[0];
            logManager = new LogManager(Model.EventLog, () => Model.ActivePlayer);
        }

        public bool Execute(string username, ClientCommand command)
        {
            lock (this)
            {
                try
                {
                    ExecuteInternal(username, command);
                    Notify();
                    return true;
                }
                catch (CommandException e)
                {
                    logManager.LogBasicEvent($"<error>{username}: {e.Message}</error>");
                    Console.WriteLine(e.ToString());
                    Notify();
                    return false;
                }
            }
        }

        internal void ExecuteInternal(string username, ClientCommand command)
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

                    logManager.LogBasicEvent($@"<spans>
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

                    logManager.LogBasicEvent($@"<spans>
                        <player>{username}</player>
                        <if you='leave' them='leaves'>{username}</if>
                        <run>the game.</run>
                    </spans>");
                    break;

                case ConfigureGameCommand configureGame:
                    if (Model.IsStarted) throw new CommandException("The game is already in progress.");

                    Model.KingdomSet = configureGame.KingdomSet;
                    Model.KingdomPreset = configureGame.KingdomPreset ?? All.Presets.BySet(Model.KingdomSet).Keys.First();

                    ConfigureKingdom();

                    break;

                case StartGameCommand startGame:
                    if (Model.IsStarted) throw new CommandException("The game is already in progress.");
                    if (!Model.Players.Contains(username)) throw new CommandException("You are not in the game.");
                    if (Model.Players.Length < 2) throw new CommandException("Not enough players.");

                    Model.KingdomSet = startGame.KingdomSet;
                    Model.KingdomPreset = startGame.KingdomPreset ?? All.Presets.BySet(Model.KingdomSet).Keys.First();

                    ConfigureKingdom();
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
                        if (Model.CurrentPhase > Phase.Action) throw new CommandException("The Action phase is over.");
                        if (Model.ActionsRemaining < 1) throw new CommandException("You have no remaining actions.");
                    }
                    else if (card.Types.Contains(CardType.Treasure))
                    {
                        if (Model.CurrentPhase > Phase.Treasure) throw new CommandException("The first part of the Buy phase is over.");
                    }

                    BeginBackgroundTask(playCard.Id, _ => PlayCardsPhasedAsync(username, card));

                    break;

                case PlayAllTreasuresCommand _:
                    if (!Model.IsStarted) throw new CommandException("The game has not begun.");
                    if (Model.IsFinished) throw new CommandException("The game is over.");
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");
                    if (Model.ExecutingBackgroundTasks) throw new CommandException("Another card is already being played.");

                    if (Model.CurrentPhase > Phase.Treasure) throw new CommandException("The first part of the Buy phase is over.");

                    var cards = Model.Hands[username].Select(All.Cards.ByName).OfType<ITreasureCard>().ToArray();

                    BeginBackgroundTask(username, _ => PlayCardsPhasedAsync(username, cards));

                    break;

                case BuyCardCommand buyCard:
                    if (!Model.IsStarted) throw new CommandException("The game has not begun.");
                    if (Model.IsFinished) throw new CommandException("The game is over.");
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");
                    if (Model.ExecutingBackgroundTasks) throw new CommandException("A card is currently being played.");

                    if (Model.BuysRemaining < 1) throw new CommandException("You have no remaining buys.");
                    if (!Model.Supply.ContainsKey(buyCard.Id)) throw new CommandException($"Card {buyCard.Id} is not in the supply.");
                    if (Model.Supply[buyCard.Id] < 1) throw new CommandException($"There are no {buyCard.Id} cards remaining.");
                    if (Model.CurrentPhase > Phase.Buy) throw new CommandException("The Buy phase is over.");

                    var boughtCard = All.Cards.ByName(buyCard.Id);
                    if (boughtCard.GetCost(Model).GreaterThan(Rules.MaxAffordableCost(Model))) throw new CommandException($"You don't have enough money to buy card {buyCard.Id}.");

                    BeginBackgroundTask(buyCard.Id, _ => BuyCardPhasedAsync(username, boughtCard));

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
            ModelUpdated?.Invoke();
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
                    backgroundTCS = new TaskCompletionSource<bool>();
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

                logManager.LogBasicEvent($"<error>{id}: {e.Message}</error>");
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
            backgroundTCS.SetResult(true);
            backgroundTCS = null;
        }

        private void CompleteBackgroundTask(Task t, string id)
        {
            Model.ExecutingBackgroundTasks = false;

            if (t.Status == TaskStatus.Faulted)
            {
                var e = t.Exception.InnerException ?? t.Exception;
                logManager.LogBasicEvent($"<error>{id}: {e.Message}</error>");
                Console.WriteLine(e.ToString());
                // rollback somehow?
            }
        }

        private void ConfigureKingdom()
        {
            Model.KingdomCards = All.Presets.BySet(Model.KingdomSet)[Model.KingdomPreset];
            Model.KingdomHasCurse = Model.KingdomCards.Any(All.Cards.UsesCurse);
            Model.KingdomHasPotion = Model.KingdomCards.Select(All.Cards.ByName).Any(c => c.GetCost(Model).Potion);
            Model.KingdomGlobalMats = new[] { "TrashMat" };
            Model.KingdomPlayerMats = Model.KingdomCards.Select(All.Cards.ByName).SelectMany(card => card.HasMat != null ? new[] { card.HasMat } : new string[0]).ToArray();
        }

        private async Task BuyCardPhasedAsync(string player, ICard card)
        {
            bool hasMore(CardType type) => Model.Hands[player].Select(All.Cards.ByName).Any(card => card.Types.Contains(type));

            // interactive preconditions
            if (Model.CurrentPhase < Phase.Buy && Model.SettingConfirmSkipPhases[player])
            {
                if (Model.CurrentPhase == Phase.Action && Model.ActionsRemaining > 0 && hasMore(CardType.Action))
                {
                    var skip = await Choose<string, bool>(
                        player, 
                        ChoiceType.YesNo, 
                        "Skip Action phase?",
                        "You have Action cards remaining. Do you really want to skip to the Buy phase?"
                    );

                    if (skip)
                    {
                        Model.SettingConfirmSkipPhases[player] = false;
                    }
                    else
                    {
                        return;
                    }
                }

                if (Model.CurrentPhase == Phase.Treasure && Model.BuysRemaining > 1 && hasMore(CardType.Treasure))
                {
                    var skip = await Choose<string, bool>(
                        player, 
                        ChoiceType.YesNo, 
                        "Skip playing remaining Treasures?",
                        "You have Treasure cards and buys remaining. Do you really want to buy a card?"
                    );

                    if (skip)
                    {
                        Model.SettingConfirmSkipPhases[player] = false;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            // commit to the buy
            await BuyCardAsync(player, card.Name);

            // advance phases
            if (Model.CurrentPhase < Phase.Buy)
            {
                Model.CurrentPhase = Phase.Buy;
            }

            var cost = card.GetCost(Model);
            Model.CoinsRemaining -= cost.Coins;
            if (cost.Potion) Model.PotionsRemaining--;

            if (--Model.BuysRemaining == 0)
            {
                await EndTurnAsync(Model.ActivePlayer);
                await BeginTurnAsync(Model.ActivePlayer);
            }
        }

        internal async Task BuyCardAsync(string player, string id)
        {            
            var buyRecord = logManager.LogComplexEvent(player, new BuyCard { Card = id });

            var instance = MoveCard(player, id, Zone.SupplyAvailable, Zone.Discard);
            NoteBuy(player, instance);

            var pileReactors = Model.SupplyTokens[id].Select(All.Effects.ByName).OfType<IReactor>();
            await TriggerReactions(buyRecord, player, Trigger.BuyCard, id, pileReactors);

            await GainCardAsync(buyRecord, player, id, Zone.SupplyAvailable, Zone.Discard);

            logManager.Save(buyRecord);
        }

        internal async Task GainCardAsync(IRecord logRecord, string player, string id, Zone from, Zone to)
        {            
            var instance = MoveCard(player, id, from, to);                    
            NoteGain(player, instance);

            var card = All.Cards.ByName(id);
            var instanceReactors = card is IReactor r ? new[]{r} : Array.Empty<IReactor>();
            await TriggerReactions(logRecord, player, Trigger.GainCard, id, instanceReactors);
        }

        private async Task PlayCardsPhasedAsync(string player, params ICard[] cards)
        {
            bool hasMore(CardType type) => Model.Hands[player].Select(All.Cards.ByName).Any(card => card.Types.Contains(type));
            var type = cards.First().Types.Contains(CardType.Action) ? CardType.Action : CardType.Treasure;
            if (type == CardType.Treasure && cards.Any(card => card.Types.Contains(CardType.Action)) ||
                type == CardType.Action && cards.Any(card => card.Types.Contains(CardType.Treasure)))
            {
                throw new CommandException("All cards must be the same type.");
            }

            // interactive preconditions
            if (type == CardType.Treasure && Model.CurrentPhase < Phase.Treasure && Model.SettingConfirmSkipPhases[player])
            {
                if (Model.CurrentPhase == Phase.Action && Model.ActionsRemaining > 0 && hasMore(CardType.Action))
                {
                    foreach (var card in cards)
                    {
                        MoveCard(player, card.Name, Zone.Hand, Zone.InPlay);
                    }
                    var skip = await Choose<string, bool>(
                        player, 
                        ChoiceType.YesNo, 
                        "Skip Action phase?",
                        "You have Action cards remaining. Do you really want to skip to the Buy phase?"
                    );
                    foreach (var card in cards)
                    {
                        MoveCard(player, card.Name, Zone.InPlay, Zone.Hand);
                    }

                    if (skip)
                    {
                        Model.SettingConfirmSkipPhases[player] = false;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            // commit to the play
            var playCardsRecord = logManager.LogComplexEvent(player, new PlayCards { Cards = cards.Names() } );
            var gainC = 0; 
            var gainP = 0;

            foreach (var card in cards)
            {                
                var (coins, potions) = await PlayCardAsync(playCardsRecord, player, card.Name, Zone.Hand);
                gainC += coins;
                gainP += potions;
            }

            if (type == CardType.Treasure)
            {
                playCardsRecord.LatestChunk.AddedCoins += gainC;
                playCardsRecord.LatestChunk.AddedPotions += gainP;
            }

            // check postconditions            
            if (Model.PreventedAttacks.Any())
            {
                playCardsRecord.LatestChunk.Lines.Add("<error>Warning: PreventedAttacks not cleared</error>");
                Model.PreventedAttacks.Clear();
            }

            if (Model.ChoosingPlayers.Any())
            {
                playCardsRecord.LatestChunk.Lines.Add($"<error>Warning: choice is stuck on '{Model.ChoicePrompt}'</error>");
                Model.ChoosingPlayers.Clear();
            }

            if (stashes.Any() || revealed.Any())
            {
                playCardsRecord.LatestChunk.Lines.Add($@"<lines>
                    <error>Warning: temp zones not drained.</error>
                    <error>Stashes: {string.Join(", ", stashes.Values.Names())}</error>
                    <error>Revealed: {string.Join(", ", revealed.Names())}</error>
                </lines>");
                stashes.Clear();
                revealed.Clear();
            }

            playCardsRecord.Update();
            logManager.Save(playCardsRecord);

            // advance phases
            if (type == CardType.Action)
            {
                Model.ActionsRemaining -= cards.Length;
                if (Model.ActionsRemaining == 0 || !hasMore(CardType.Action))
                {
                    Model.CurrentPhase = Phase.Treasure;
                }
            }

            if (type == CardType.Treasure && Model.CurrentPhase < Phase.Treasure)
            {
                Model.CurrentPhase = Phase.Treasure;
            }

            if (Model.CurrentPhase == Phase.Treasure && !hasMore(CardType.Treasure))
            {
                Model.CurrentPhase = Phase.Buy;
            }
        }

        internal async Task<(int coins, int potions)> PlayCardAsync(IRecord logRecord, string player, string id, Zone from)
        {       
            var played = MoveCard(player, id, from, Zone.InPlay);
            var card = All.Cards.ByName(id);

            var pileReactors = Model.SupplyTokens[id].Select(All.Effects.ByName).OfType<IReactor>();
            var instanceReactors = card is IReactor r ? new[]{r} : Array.Empty<IReactor>();
            var extraReactors = pileReactors.Concat(instanceReactors).ToArray();        

            if (card.Types.Contains(CardType.Attack))
            {
                await TriggerReactions(logRecord, player, Trigger.Attack, player, extraReactors);
            }
                        
            if (card.Types.Contains(CardType.Duration))
            {
                Model.PlayedWithDuration.Add(played);
            }

            var gainC = 0;
            var gainP = 0;
            var host = new CardHost(this, logRecord, player, played);
            if (card is IActionCard action)
            {
                if (player == Model.ActivePlayer)
                {
                    ActionsThisTurn++;
                }

                await action.ExecuteActionAsync(host);
            }
            else if (card is ITreasureCard treasure)
            {
                var increaseCoins = Model.GetModifiers().Select(m => m.IncreaseTreasureValue(card.Name)).Sum();
                var value = await treasure.GetValueAsync(host);

                gainC = value.Coins + increaseCoins;
                Model.CoinsRemaining += gainC;

                gainP = value.Potion ? 1 : 0;
                Model.PotionsRemaining += gainP;
            }
            else
            {
                throw new CommandException($"Only Actions and Treasures can be played.");
            }

            NotePlay(player, played);

            await TriggerReactions(logRecord, player, Trigger.PlayCard, id, extraReactors);

            return (gainC, gainP);
        }

        private void BeginGame()
        {
            var rng = new Random();

            Model.Supply = All.Cards.Base().Concat(Model.KingdomCards).ToDictionary(id => id, id => Rules.GetInitialSupply(Model.Players.Length, id));
            if (Model.KingdomHasPotion) Model.Supply["Potion"] = Rules.GetInitialSupply(Model.Players.Length, "Potion");
            Model.SupplyTokens = Model.Supply.Keys.ToDictionary(k => k, _ => new string[0]);
            Model.ActiveEffects = new List<string>();
            Model.PreventedAttacks = new HashSet<string>();
            Model.ChoosingPlayers = new Stack<string>();
            Model.Hands = Model.Players.ToDictionary(k => k, _ => new List<Instance>());
            Model.Discards = Model.Players.ToDictionary(k => k, _ => new List<Instance>());
            Model.PlayedCards = Model.Players.ToDictionary(k => k, _ => new List<Instance>());
            Model.PlayedWithDuration = new HashSet<Instance>();
            Model.PlayedLastTurn = new HashSet<Instance>();
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
            Model.SettingConfirmSkipPhases = Model.Players.ToDictionary(k => k, _ => true);
            Model.SettingKeepHandSorted = Model.Players.ToDictionary(k => k, _ => true);
            Model.IsStarted = true;

            foreach (var player in Model.Players)
            {
                turnNumbers[player] = 0;
                for (var i = 0; i < 5; i++)
                {
                    DrawCardIfAny(player);
                }
            }
            
            Model.ActivePlayer = Model.Players[rng.Next(Model.Players.Length)];
        }

        internal async Task BeginTurnAsync(string player)
        {
            if (Model.IsFinished) return; // XXX what case does this cover?

            var turnNumber = ++turnNumbers[player];
            turns[player] = new TurnRecord();

            ActionsThisTurn = 0;
            Model.ActionsRemaining = 1;
            Model.BuysRemaining = 1;
            Model.CoinsRemaining = 0;
            Model.PotionsRemaining = 0;
            Model.CurrentPhase = Phase.Action;
            Model.PlayedLastTurn = new HashSet<Instance>(Model.PlayedCards[player]);

            var beginTurnRecord = logManager.LogComplexEvent(player, new BeginTurn { TurnNumber = turnNumber });
            await TriggerReactions(beginTurnRecord, player, Trigger.BeginTurn, player);
            logManager.Save(beginTurnRecord);

            // advance phases
            if (!Model.Hands[player].Select(All.Cards.ByName).Any(card => card.Types.Contains(CardType.Action)))
            {
                Model.CurrentPhase = Phase.Treasure;
                if (!Model.Hands[player].Select(All.Cards.ByName).Any(card => card.Types.Contains(CardType.Treasure)))
                {
                    Model.CurrentPhase = Phase.Buy;
                }
            }

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
                        logManager.LogBasicEvent($"<error>{botPlayer}: Sorry! I'm just a bot.</error>");
                    }
                });
            }
        }

        internal async Task EndTurnAsync(string player)
        {
            Model.CurrentPhase = Phase.Cleanup;

            var endTurnRecord = logManager.LogComplexEvent(player, new Cleanup());

            var discard = Model.Discards[player];
            var inPlay = Model.PlayedCards[player];

            var toDiscard = new List<Instance>();
            foreach (var instance in inPlay.OrderBy(i => All.Cards.ByName(i) is IReactor ? 0 : 1).ToList())
            {
                var card = All.Cards.ByName(instance);
                if (!card.Types.Contains(CardType.Duration) || !Model.PlayedWithDuration.Contains(instance))
                {
                    inPlay.Remove(instance);
                    discard.Add(instance);

                    var reactors = card is IReactor r ? new[]{r} : Array.Empty<IReactor>();
                    await TriggerReactions(endTurnRecord, player, Trigger.DiscardFromPlay, instance.Id, reactors);
                }
            }

            Model.ActiveEffects.Clear();

            var hand = Model.Hands[player];
            while (hand.Any())
            {
                MoveCard(player, hand[0], Zone.Hand, Zone.Discard);
            }

            var nextHandSize = Model.GetModifiers().FirstOrDefault(m => m.NextHandSize.HasValue)?.NextHandSize.Value ?? 5;
            var reshuffled = false;
            for (var i = 0; i < nextHandSize; i++)
            {
                reshuffled = reshuffled | ReshuffleIfEmpty(player);
                DrawCardIfAny(player);
            }
            if (reshuffled)
            {
                endTurnRecord.LatestChunk.Reshuffled = true;
                endTurnRecord.Update();
            }
            
            // display the EOT record only if it's got content
            if (endTurnRecord.HasContent())
            {
                logManager.Save(endTurnRecord);
            }
            else
            {
                logManager.ClearEntry(endTurnRecord);
            }

            if (Model.Supply["Province"] == 0 || Model.Supply.Values.Where(v => v == 0).Count() >= 3)
            {
                EndGame();
            }
            else
            {
                Model.PreviousPlayer = Model.ActivePlayer;

                if (Model.NextPlayer == null)
                {
                    var nextPlayerIndex = Array.FindIndex(Model.Players, e => e.Equals(Model.ActivePlayer)) + 1;
                    if (nextPlayerIndex >= Model.Players.Length)
                    {
                        nextPlayerIndex = 0;
                    }
                    Model.NextPlayer = Model.Players[nextPlayerIndex];
                }

                if (ExtraTurns.Any())
                {
                    Model.ActivePlayer = ExtraTurns.Dequeue();
                }
                else
                {
                    Model.ActivePlayer = Model.NextPlayer;
                    Model.NextPlayer = null;
                }
            }
        }

        private void EndGame()
        {
            Model.IsFinished = true;

            logManager.LogBasicEvent($@"<bold>--- Game over ---</bold>");

            foreach (var pair in Model.Players.Select(player => (s: All.Rules.CalculateScore(Model, player), t: turnNumbers[player])))
            {
                var builder = new StringBuilder();
                builder.AppendLine("<lines>");
                    builder.AppendLine($"<spans><player>{pair.s.Player}</player><run>scored:</run></spans>");                    
                    foreach (var group in pair.s.Subtotals)
                    {
                        builder.AppendLine("<spans>");
                        builder.AppendLine($"<card>{group.card}</card>");
                        builder.AppendLine($"<run>x{group.count}: {group.subtotal} VP</run>");
                        builder.AppendLine("</spans>");
                    }                
                    builder.AppendLine($"<run>Total: {pair.s.Total} Victory Points in {pair.t} turns.</run>");
                builder.AppendLine("</lines>");

                logManager.LogBasicEvent(builder.ToString());
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
                MoveCard(player, id, Zone.Deck, to ?? Zone.Hand);
            }
            else
            {
                id = deck[0].Id;
                MoveCard(player, id, Zone.Deck, to ?? Zone.Hand);
            }

            return id;
        }

        internal Instance MoveCard(string player, string id, Zone from, Zone to)
        {
            Instance instance;

            switch (from.Name)
            {
                case ZoneName.Attached when from.Param is Instance target:
                    instance = Model.Attachments[target];
                    Model.Attachments.Remove(target);
                    break;

                case ZoneName.Create:
                    instance = Instance.Of(id);
                    break;

                case ZoneName.DeckBottom:
                    if (Model.Decks[player].Last().Id != id) throw new CommandException($"No {id} card on bottom of deck.");
                    instance = Model.Decks[player].Last();
                    break;

                case ZoneName.Deck:
                case ZoneName.DeckTop:
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

                case ZoneName.PlayerMat when from.Param is string fromMat:
                    if (!Model.PlayerMatCards[player][fromMat].Contains(id)) throw new CommandException($"No {id} card on mat {fromMat}.");
                    instance = Model.PlayerMatCards[player][fromMat].ExtractLast(id);
                    break;

                case ZoneName.Revealed:
                    if (!revealed.Contains(id)) throw new CommandException($"No {id} card has been revealed.");
                    instance = revealed.Extract(id);
                    break;

                case ZoneName.Stash when from.Param is int stashID:
                    instance = stashes[stashID];
                    stashes.Remove(stashID);
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
                case ZoneName.Attached when to.Param is Instance target:
                    Model.Attachments[target] = instance;
                    break;

                case ZoneName.DeckBottom:
                    Model.Decks[player].Add(instance);
                    break;

                case ZoneName.Deck:
                case ZoneName.DeckTop:
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

                case ZoneName.PlayerMat when to.Param is string toMat:
                    Model.PlayerMatCards[player][toMat].Add(instance);
                    break;

                case ZoneName.Revealed:
                    revealed.Add(instance);
                    break;

                case ZoneName.Stash when to.Param is int stashID:
                    stashes[stashID] = instance;
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
                ZoneName.Deck => Model.Decks[player].Count(),
                ZoneName.DeckBottom => Model.Decks[player].Take(1).Count(),
                ZoneName.DeckTop when source.Param is int n => Model.Decks[player].Take(n).Count(),
                ZoneName.Discard => Model.Discards[player].Count(),
                ZoneName.Hand => Model.Hands[player].Count,
                ZoneName.InPlay => Model.PlayedCards.Count,
                ZoneName.PlayerMat when source.Param is string mat => Model.PlayerMatCards[player][mat].Count,
                ZoneName.Supply when source.Param is (bool includeAvailable, bool includeEmpty) => Model.Supply.Keys.Count(id =>
                {
                    if (Model.Supply[id] > 0) 
                    {
                        return includeAvailable;
                    }
                    else
                    {
                        return includeEmpty;
                    }
                }),
                ZoneName.Trash => Model.MatCards["TrashMat"].Count,
                _ => throw new CommandException($"Unsupported counting zone {source}")
            };
        }

        internal Instance[] GetInstances(string player, Zone source, Action onShuffle = null)
        {
            onShuffle = onShuffle ?? new Action(() => {;});

            if (source is Zone { Name: ZoneName.DeckTop, Param: int topN } && Model.Decks[player].Count < topN)
            {
                var setAside = Model.Decks[player].ToArray();
                Model.Decks[player].Clear();                
                ReshuffleIfEmpty(player);
                Model.Decks[player].InsertRange(0, setAside);
                onShuffle();
            }

            return source.Name switch 
            {
                ZoneName.Deck => Model.Decks[player].ToArray(),
                ZoneName.DeckBottom => new[] { Model.Decks[player].Last() },
                ZoneName.DeckTop when source.Param is int n => Model.Decks[player].Take(n).ToArray(),
                ZoneName.Discard => Model.Discards[player].ToArray(),
                ZoneName.Hand => Model.Hands[player].ToArray(),
                ZoneName.InPlay => Model.PlayedCards[player].ToArray(),
                ZoneName.PlayerMat when source.Param is string mat => Model.PlayerMatCards[player][mat].ToArray(),
                ZoneName.RecentBuys => turns[player].Buys.ToArray(),
                ZoneName.RecentGains => turns[player].Gains.ToArray(),
                ZoneName.RecentPlays => turns[player].Plays.ToArray(),
                ZoneName.Trash => Model.MatCards["TrashMat"].ToArray(),
                _ => throw new CommandException($"Unknown instance zone {source}")
            };
        }

        internal string[] GetCards(string player, Zone source, Action onShuffle = null)
        {
            return source.Name switch 
            {
                ZoneName.Supply when source.Param is (bool includeAvailable, bool includeEmpty) => Model.Supply.Keys.Where(id => 
                {
                    if (Model.Supply[id] > 0) 
                    {
                        return includeAvailable;
                    }
                    else
                    {
                        return includeEmpty;
                    }
                }).ToArray(),
                _ => GetInstances(player, source, onShuffle).Names()
            };
        }

        internal void SetCardOrder(string player, string[] cards, Zone destination)
        {
            void reorderDeckTop(int n)
            {
                var newOrder = new List<Instance>();
                for (var i = 0; i < n; i++)
                {
                    var instance = Model.Decks[player].Take(n).First(inst => inst.Id == cards[i] && !newOrder.Contains(inst)); 
                    newOrder.Add(instance);
                }
                for (var i = 0; i < n; i++)
                {
                    Model.Decks[player][i] = newOrder[i];
                }
            }

            switch (destination.Name)
            {
                case ZoneName.Deck:
                    reorderDeckTop(cards.Length);
                    break;

                case ZoneName.DeckTop when destination.Param is int n:
                    reorderDeckTop(n);
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

                // unordered zones
                case ZoneName.Hand:
                case ZoneName.InPlay:
                case ZoneName.PlayerMat:
                case ZoneName.Stash:
                case ZoneName.Trash:
                    break;

                default:
                    throw new CommandException($"Unsupported reorder zone {destination}");
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

            if (Model.ChoosingPlayers.Any()) throw new Exception("choice stack can occur somehow");
            Model.ChoosingPlayers.Push(player);           
             
            inputTCS = new TaskCompletionSource<string>();
            Notify();
            var output = await inputTCS.Task;

            Model.ChoosingPlayers.Pop();

            return JsonSerializer.Deserialize<TOutput>(output);
        }

        internal async Task TriggerReactions(IRecord addToRecord, string player, Trigger type, string parameter, IEnumerable<IReactor> extraReactors = null)
        {
            var globalHost = new TriggerHost(this, addToRecord, player, type, parameter);

            var globalReactors = Model.ActiveEffects
                .Select(All.Effects.ByName)
                .OfType<IReactor>()
                .ToList();            

            while (globalReactors.Any())
            {
                var reactor = globalReactors.First();
                globalReactors.Remove(reactor);                
                await reactor.ExecuteReactionAsync(globalHost, Zone.InPlay, type, parameter);
            }

            var localReactors = extraReactors?.ToList();
            while (localReactors?.Any()??false)
            {
                var reactor = localReactors.First();
                localReactors.Remove(reactor);
                await reactor.ExecuteReactionAsync(globalHost, Zone.This, type, parameter);
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
                    
                    var ownedRecord = addToRecord.CreateSubrecord(owningPlayer, true);
                    var ownedHost = new CardHost(this, ownedRecord, owningPlayer, ownedReactor.instance);
                    await ownedReactor.reactor.ExecuteReactionAsync(ownedHost, ownedReactor.zone, type, parameter);
                }
            }
        }

        private void NoteBuy(string player, Instance instance)
        {
            if (player == Model.ActivePlayer)
            {
                turns[player].Buys.Add(instance);
            }
        }

        private void NoteGain(string player, Instance instance)
        {
            if (player == Model.ActivePlayer)
            {
                turns[player].Gains.Add(instance);
            }
        }

        private void NotePlay(string player, Instance instance)
        {
            if (player == Model.ActivePlayer)
            {
                turns[player].Plays.Add(instance);
            }
        }
    }
}