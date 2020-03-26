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
            switch (command)
            {
                case JoinGameCommand _:
                    if (Model.Players.Contains(username))
                    {
                        throw new CommandException($"You are already in the game.");
                    }
                    else
                    {
                        Model.Players = Model.Players.Append(username).ToArray();
                    }
                    break;

                case LeaveGameCommand _:
                    if (Model.Players.Contains(username))
                    {
                        Model.Players = Model.Players.Except(new[]{username}).ToArray();
                    }
                    else
                    {
                        throw new CommandException($"You are not in the game.");                        
                    }
                    break;

                case var unknown:
                    throw new CommandException($"Unrecognised command {unknown}");
            }
        }
    }
}