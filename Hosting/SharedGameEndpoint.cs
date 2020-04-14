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

                Notify();
            }

            return games[name];
        }

        public void CreateDemoGame(string name)
        {
            games[name] = new SharedGame(name);
            games[name].SummaryUpdated += Notify;

            games[name].Execute("demo", new SetDemoCommand { Seq = 0 });
            games[name].Execute("agatha", new JoinGameCommand { Seq = 1 });
            games[name].Execute("agatha", new ChatCommand { Seq = 2, Message = "Hello, kingdom!" });
            games[name].Execute("beto", new JoinGameCommand { Seq = 3 });
            games[name].Execute("beto", new ChatCommand { Seq = 4, Message = "Don't cramp my style." });
            games[name].Execute("cawdelia", new JoinGameCommand { Seq = 5 });
            games[name].Execute("cawdelia", new ChatCommand { Seq = 6, Message = "Nevermore." });
            games[name].Execute("demo", new JoinGameCommand { Seq = 7 });
            games[name].Execute("demo", new StartGameCommand { Seq = 8 });
            games[name].Execute("demo", new PlayCardCommand { Seq = 9, Id = "Copper" });
            games[name].Execute("demo", new PlayCardCommand { Seq = 10, Id = "Copper" });
            games[name].Execute("demo", new PlayCardCommand { Seq = 11, Id = "Copper" });
            games[name].Execute("demo", new PlayCardCommand { Seq = 12, Id = "Copper" });
            games[name].Execute("demo", new PlayCardCommand { Seq = 13, Id = "Copper" });
            games[name].Execute("demo", new BuyCardCommand { Seq = 14, Id = "Mine" });
        }
    }
}