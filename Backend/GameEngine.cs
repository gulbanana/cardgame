using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cardgame
{
    public class GameEngine : IActionHost
    {
        public readonly GameModel Model;
        public event Action ActionCompleted;

        public GameEngine()
        {
            Model = new GameModel();
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
                case ChatCommand chat:
                    if (chat.Message.Length > LogEntry.MAX) throw new CommandException("Chat message too long.");

                    Model.ChatLog.Add(new LogEntry { Username = username, Message = chat.Message });

                    break;

                case JoinGameCommand _:
                    if (Model.IsStarted) throw new CommandException("The game is already in progress.");
                    if (Model.Players.Contains(username)) throw new CommandException("You are already in the game.");
                    if (Model.Players.Length >= 4) throw new CommandException("The game is full.");

                    Model.Players = Model.Players.Append(username).ToArray();

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

                    LogEvent($@"<spans>
                        <player>{username}</player>
                        <if you='leave' them='leaves'>{username}</if>
                        <run>the game.</run>
                    </spans>");
                    break;

                case StartGameCommand _:
                    if (Model.IsStarted) throw new CommandException("The game is already in progress.");
                    if (!Model.Players.Contains(username)) throw new CommandException("You are not in the game.");
                    if (Model.Players.Length < 2) throw new CommandException("Not enough players.");

                    BeginGame();
                    BeginTurn();

                    break;

                case PlayCardCommand playCard:
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");
                    if (!Model.Hands[username].Contains(playCard.Id)) throw new CommandException($"You don't have a {playCard.Id} card in your hand.");
                    if (!Cards.All.ByName.ContainsKey(playCard.Id)) throw new CommandException($"Card {playCard.Id} is not implemented.");

                    PlayCard(username, playCard.Id);

                    break;

                case BuyCardCommand buyCard:
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");
                    if (Model.BuysRemaining < 1) throw new CommandException("You have no remaining buys.");
                    if (Model.CardStacks[buyCard.Id] < 1) throw new CommandException($"There are no {buyCard.Id} cards remaining.");

                    BuyCard(username, buyCard.Id);

                    break;

                case EndTurnCommand _:
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");

                    EndTurn();
                    BeginTurn();
                    break;

                case var unknown:
                    throw new CommandException($"Unrecognised command {unknown}");
            }

            Model.Seq++;
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
                for (var i = 0; i < 5; i++)
                {
                    DrawCard(player);
                }
            }
            
            Model.ActivePlayer = Model.Players.Contains("demo") ? "demo" : Model.Players[rng.Next(Model.Players.Length)];
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
                var first = hand[0];
                hand.RemoveAt(0);
                discard.Add(first);
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

            if (Model.ActivePlayer != "demo")
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

        private bool DrawCard(string player)
        {
            var deck = Model.Decks[player];
            var hand = Model.Hands[player];

            var first = deck[0];
            deck.RemoveAt(0);
            hand.Add(first);
            
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

        private void BuyCard(string player, string id)
        {
            var boughtCard = Cards.All.ByName[id];
            if (boughtCard.Cost > Model.MoneyRemaining) throw new CommandException($"You don't have enough money to buy card {id}.");

            Model.CardStacks[id]--;
            Model.Discards[player].Add(id);
            Model.MoneyRemaining -= boughtCard.Cost;
            Model.BuysRemaining -= 1;

            LogEvent($@"<spans>
                <player>{player}</player>
                <if you='buy' them='buys'>{player}</if>
                <card>{id}</card>
                <run>.</run>
            </spans>");

            if (Model.BuysRemaining == 0)
            {
                EndTurn();
                BeginTurn();
            }
            else
            {
                SkipBuyIfNoCash();
            }
        }

        private void PlayCard(string player, string id)
        {
            Model.Hands[player].Remove(id);
            Model.PlayedCards.Add(id);

            var playedCard = Cards.All.ByName[id];                    
            switch (playedCard.Type)
            {                    
                case CardType.Action when playedCard is Cards.ActionCardModel action:
                    if (Model.BuyPhase) throw new CommandException($"The Action phase is over.");
                    if (Model.ActionsRemaining < 1) throw new CommandException("You have no remaining actions.");

                    Model.ActionsRemaining--;
                    Model.IsExecutingAction = true;
                    action.ExecuteAsync(this).ContinueWith(CompleteAction);
                    break;
                
                case CardType.Treasure when playedCard is Cards.TreasureCardModel treasure:
                    if (!Model.BuyPhase)
                    {
                        Model.BuyPhase = true;
                        SkipBuyIfNoCash();
                    }
                    Model.MoneyRemaining += treasure.Value;
                    break;

                case CardType.Victory:
                case CardType.Curse:
                default:
                    throw new CommandException($"You can't play {playedCard.Type} cards.");
            }

            LogEvent($@"<spans>
                <player>{player}</player>
                <if you='play' them='plays'>{player}</if>
                <card>{id}</card>
                <run>.</run>
            </spans>");
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
            }

            ActionCompleted?.Invoke();
        }

        void IActionHost.DrawCards(int n)
        {
            var reshuffled = false;
            for (var i = 0; i < n; i++)
            {
                reshuffled = reshuffled | DrawCard(Model.ActivePlayer);
            }
            if (reshuffled)
            {
                LogPartialEvent($@"<spans>
                    <run>...</run>
                    <run>(</run>
                    <if you='you reshuffle.' them='reshuffling.'>{Model.ActivePlayer}</if>
                    <run>)</run>
                </spans>");
            }

            LogPartialEvent($@"<spans>
                <run>...</run>
                <if you='you draw' them='drawing'>{Model.ActivePlayer}</if>
                <run>{n} cards.</run>
            </spans>");
        }

        void IActionHost.AddActions(int n)
        {
            Model.ActionsRemaining += n;

            LogPartialEvent($@"<spans>
                <run>...</run>
                <if you='you get' them='getting'>{Model.ActivePlayer}</if>
                <run>+{n} actions.</run>
            </spans>");
        }

        void IActionHost.AddBuys(int n)
        {
            Model.BuysRemaining += n;

            LogPartialEvent($@"<spans>
                <run>...</run>
                <if you='you get' them='getting'>{Model.ActivePlayer}</if>
                <run>+{n} buys.</run>
            </spans>");
        }

        void IActionHost.AddMoney(int n)
        {
            Model.MoneyRemaining += n;

            LogPartialEvent($@"<spans>
                <run>...</run>
                <if you='you get' them='getting'>{Model.ActivePlayer}</if>
                <run>+${n}.</run>
            </spans>");
        }
    }
}