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

        public GameEngine(GameModel model)
        {
            Model = model;
        }

        public void Execute(string username, ClientCommand command)
        {
            if (Model.Seq != command.Seq)
            {
                throw new CommandException($"Incorrect sequence number.");
            }

            switch (command)
            {
                case JoinGameCommand _:
                    if (Model.Players.Contains(username)) throw new CommandException($"You are already in the game.");
                    Model.Players = Model.Players.Append(username).ToArray();
                    break;

                case LeaveGameCommand _:
                    if (!Model.Players.Contains(username)) throw new CommandException($"You are not in the game.");
                    Model.Players = Model.Players.Except(new[]{username}).ToArray();
                    break;

                case ChatCommand chat:
                    if (chat.Message.Length > LogEntry.MAX) throw new CommandException($"Chat message too long.");
                    Model.ChatLog.Add(new LogEntry { Username = username, Message = chat.Message });
                    break;

                case var unknown:
                    throw new CommandException($"Unrecognised command {unknown}");
            }

            Model.Seq++;
        }
    }
}