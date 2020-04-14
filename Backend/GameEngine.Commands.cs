using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cardgame
{
    public partial class GameEngine
    {
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

                    Model.IsDemo = true;

                    break;

                case SetNextPlayerCommand nextPlayer:
                    if (!Model.IsDemo) throw new CommandException("Game must be in demo mode.");

                    Model.DemoNextActive = nextPlayer.Player;

                    break;

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
                    if (Model.IsFinished) throw new CommandException("The game is over.");
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");
                    if (!Model.Hands[username].Contains(playCard.Id)) throw new CommandException($"You don't have a {playCard.Id} card in your hand.");
                    if (!Cards.All.ByName.ContainsKey(playCard.Id)) throw new CommandException($"Card {playCard.Id} is not implemented.");

                    PlayCard(username, playCard.Id);

                    break;

                case PlayAllTreasuresCommand _:
                    if (Model.IsFinished) throw new CommandException("The game is over.");
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");

                    foreach (var card in Model.Hands[username].Select(id => Cards.All.ByName[id]).OfType<Cards.TreasureCardModel>().ToList())
                    {
                        PlayCard(username, card.Name);
                    }

                    break;

                case BuyCardCommand buyCard:
                    if (Model.IsFinished) throw new CommandException("The game is over.");
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");
                    if (Model.BuysRemaining < 1) throw new CommandException("You have no remaining buys.");
                    if (Model.CardStacks[buyCard.Id] < 1) throw new CommandException($"There are no {buyCard.Id} cards remaining.");

                    BuyCard(username, buyCard.Id);

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
            var boughtCard = Cards.All.ByName[id];
            if (boughtCard.Cost > Model.MoneyRemaining) throw new CommandException($"You don't have enough money to buy card {id}.");

            Model.CardStacks[id]--;
            Model.Discards[player].Add(id);
            Model.MoneyRemaining -= boughtCard.Cost;
            Model.BuysRemaining -= 1;

            LogEvent($@"<spans>
                <player>{player}</player>
                <if you='buy' them='buys'>{player}</if>
                <card suffix='.'>{id}</card>
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

            LogEvent($@"<spans>
                <player>{player}</player>
                <if you='play' them='plays'>{player}</if>
                <card suffix='.'>{id}</card>
            </spans>");

            var playedCard = Cards.All.ByName[id];                    
            switch (playedCard.Type)
            {                    
                case CardType.Action when playedCard is Cards.ActionCardModel action:
                    if (Model.BuyPhase) throw new CommandException($"The Action phase is over.");
                    if (Model.ActionsRemaining < 1) throw new CommandException("You have no remaining actions.");

                    Model.ActionsRemaining--;
                    Model.IsExecutingAction = true;
                    var host = new ActionHost(this, Model.ActivePlayer);
                    action.ExecuteActionAsync(host).ContinueWith(CompleteAction);
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

            ActionUpdated?.Invoke();
        }
    }
}