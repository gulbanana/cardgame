using System.Collections.Generic;
using System.Linq;

namespace Cardgame.Hosting
{
    // adapter implementing game protocol using shared memory
    class SharedGameEndpoint : EndpointBase<GameSummary[]>, IGameEndpoint
    {
        private readonly Dictionary<string, SharedGame> games;

        public SharedGameEndpoint()
        {
            games = new Dictionary<string, SharedGame>();
        }

        protected override GameSummary[] GetModel()
        {
            return games.Values.Select(game => game.Summary).ToArray();
        }

        public IGame FindGame(string name)
        {
            if (!games.ContainsKey(name))
            {
                games[name] = new SharedGame(name);
                games[name].SummaryUpdated += Notify;

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

                Notify();
            }

            return games[name];
        }
    }
}