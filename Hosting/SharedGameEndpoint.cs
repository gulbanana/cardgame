using System.Collections.Generic;
using System.Linq;

namespace Cardgame
{
    // adapter implementing game protocol using shared memory
    class SharedGameEndpoint : IGameEndpoint
    {
        private readonly Dictionary<string, SharedGame> games;

        public SharedGameEndpoint()
        {
            games = new Dictionary<string, SharedGame>();
        }

        public IGame FindGame(string name)
        {
            if (!games.ContainsKey(name))
            {
                games[name] = new SharedGame();

                if (name.StartsWith("demo"))
                {
                    games[name].Execute("agatha", new JoinGameCommand { Seq = 0 });
                    games[name].Execute("agatha", new ChatCommand { Seq = 1, Message = "Hello, kingdom!" });
                    games[name].Execute("beto", new JoinGameCommand { Seq = 2 });
                    games[name].Execute("beto", new ChatCommand { Seq = 3, Message = "Don't cramp my style." });
                    games[name].Execute("cordelia", new JoinGameCommand { Seq = 4 });
                    games[name].Execute("cordelia", new ChatCommand { Seq = 5, Message = "Hello, kingdom!" });
                    games[name].Execute("demo", new JoinGameCommand { Seq = 6 });
                    games[name].Execute("demo", new StartGameCommand { Seq = 7 });
                }
            }

            return games[name];
        }

        public GameSummary[] ListGames()
        {
            return games.Keys.Select(name => 
            {
                var game = games[name].GetModel();
                return new GameSummary
                {
                    Name = name,
                    Players = game.Players,
                    Status = game.IsStarted ? "in progress" : "waiting to start"
                };
            }).ToArray();
        }
    }
}