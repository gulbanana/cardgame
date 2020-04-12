using System.Collections.Generic;
using System.Linq;

namespace Cardgame
{
    public class GameEngine
    {
        public readonly GameModel Model;

        public GameEngine()
        {
            Model = new GameModel();
        }

        public void Execute(string username, ClientCommand command)
        {
            if (Model.Seq != command.Seq)
            {
                throw new CommandException($"Incorrect sequence number.");
            }

            switch (command)
            {
                case ChatCommand chat:
                    if (chat.Message.Length > LogEntry.MAX) throw new CommandException($"Chat message too long.");

                    Model.ChatLog.Add(new LogEntry { Username = username, Message = chat.Message });

                    break;

                case JoinGameCommand _:
                    if (Model.IsStarted) throw new CommandException($"The game is already in progress.");
                    if (Model.Players.Contains(username)) throw new CommandException($"You are already in the game.");

                    Model.Players = Model.Players.Append(username).ToArray();

                    LogEvent($"<spans><player>{username}</player><run> joined the game.</run></spans>");
                    break;

                case LeaveGameCommand _:
                    if (Model.IsStarted) throw new CommandException($"The game is already in progress.");
                    if (!Model.Players.Contains(username)) throw new CommandException($"You are not in the game.");

                    Model.Players = Model.Players.Except(new[]{username}).ToArray();

                    LogEvent($"<spans><player>{username}</player><run> left the game.</run></spans>");
                    break;

                case StartGameCommand _:
                    if (Model.IsStarted) throw new CommandException($"The game is already in progress.");

                    Model.IsStarted = true;
                    Model.Hands = Model.Players.ToDictionary(k => k, _ => new List<string> { "Copper", "Copper", "Copper", "Estate", "Estate" });
                    Model.Decks = Model.Players.ToDictionary(k => k, _ => new List<string>());
                    Model.Discards = Model.Players.ToDictionary(k => k, _ => new List<string>());
                    Model.KingdomCards = new[] // XX pick at random instead!
                    {
                        "Cellar", "Market", "Mine", "Remodel", "Moat",
                        "Smithy", "Village", "Woodcutter", "Workshop", "Militia"
                    };

                    Model.PlayedCards = new List<string>();
                    Model.ActivePlayer = username; // XXX pick at random instead!
                    BeginTurn();

                    LogEvent($"<run>The game began.</run>");
                    break;

                case PlayCardCommand playCard:
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");
                    if (!Model.Hands[username].Contains(playCard.Id)) throw new CommandException($"You don't have a {playCard.Id} card in your hand.");
                    if (!Cards.All.ByName.ContainsKey(playCard.Id)) throw new CommandException($"Card {playCard.Id} is not implemented.");

                    Model.Hands[username].Remove(playCard.Id);
                    Model.PlayedCards.Add(playCard.Id);

                    var playedCard = Cards.All.ByName[playCard.Id];                    
                    switch (playedCard.Type)
                    {                    
                        case CardType.Action when playedCard is Cards.KingdomCardModel action:
                            if (Model.BuyPhase) throw new CommandException($"The Action phase is over.");
                            if (Model.ActionsRemaining < 1) throw new CommandException("You have no remaining actions.");
                            throw new System.NotImplementedException();
                        
                        case CardType.Treasure when playedCard is Cards.TreasureCardModel treasure:
                            if (!Model.BuyPhase)
                            {
                                Model.BuyPhase = true;
                            }
                            Model.MoneyRemaining += treasure.Value;
                            break;

                        case CardType.Victory:
                        case CardType.Curse:
                        default:
                            throw new CommandException($"You can't play {playedCard.Type} cards.");
                    }

                    LogEvent($"<spans><player>{username}</player><run> played </run><card>{playCard.Id}</card><run>.</run></spans>");
                    break;

                case BuyCardCommand buyCard:
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");
                    if (Model.BuysRemaining < 1) throw new CommandException("You have no remaining buys.");

                    var boughtCard = Cards.All.ByName[buyCard.Id];
                    if (boughtCard.Cost > Model.MoneyRemaining) throw new CommandException($"You don't have enough money to buy card {buyCard.Id}.");

                    // XXX reduce count of stack 
                    Model.Discards[username].Add(buyCard.Id);
                    Model.MoneyRemaining -= boughtCard.Cost;

                    LogEvent($"<spans><player>{username}</player><run> bought </run><card>{buyCard.Id}</card><run>.</run></spans>");
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

        private void BeginTurn()
        {
            Model.BuyPhase = false;
            Model.ActionsRemaining = 1;
            Model.BuysRemaining = 1;
            Model.MoneyRemaining = 0;
            Model.PlayedCards.Clear();
        }

        private void EndTurn()
        {
            var discard = Model.Discards[Model.ActivePlayer];
            while (Model.PlayedCards.Any())
            {
                var first = Model.PlayedCards[0];
                Model.PlayedCards.RemoveAt(0);
                discard.Append(first);
            }
        }
    }
}