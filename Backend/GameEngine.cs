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
                    Model.ActivePlayer = username; // XXX pick at random instead!
                    Model.KingdomCards = new[]
                    {
                        "Cellar", "Market", "Mine", "Remodel", "Moat",
                        "Smithy", "Village", "Woodcutter", "Workshop", "Militia"
                    };

                    LogEvent($"<run>The game began.</run>");
                    break;

                case PlayCardCommand playCard:
                    if (Model.ActivePlayer != username) throw new CommandException("You are not the active player.");

                    LogEvent($"<spans><player>{username}</player><run> played </run><card>{playCard.Id}</card><run>.</run></spans>");
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
    }
}