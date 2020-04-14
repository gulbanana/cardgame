using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                games[name].GameEnded += OnGameEnded;

                Notify();
            }

            return games[name];
        }

        public void CreateDemoGame(string name)
        {
            games[name] = new SharedGame(name);
            games[name].SummaryUpdated += Notify;
            games[name].GameEnded += OnGameEnded;

            int seq = 0;
            games[name].Execute("demo", new SetDemoCommand { Seq = seq++ });
            games[name].Execute("agatha", new JoinGameCommand { Seq = seq++ });
            games[name].Execute("agatha", new ChatCommand { Seq = seq++, Message = "Hello, kingdom!" });
            games[name].Execute("beto", new JoinGameCommand { Seq = seq++ });
            games[name].Execute("beto", new ChatCommand { Seq = seq++, Message = "Don't cramp my style." });
            games[name].Execute("cawdelia", new JoinGameCommand { Seq = seq++ });
            games[name].Execute("cawdelia", new ChatCommand { Seq = seq++, Message = "Nevermore." });
            games[name].Execute("demo", new JoinGameCommand { Seq = seq++ });
            games[name].Execute("demo", new SetNextPlayerCommand { Seq = seq++, Player = "demo" });
            games[name].Execute("demo", new StartGameCommand { Seq = seq++ });

            games[name].Execute("demo", new SetNextPlayerCommand { Seq = seq++, Player = "agatha" });
            games[name].Execute("demo", new PlayCardCommand { Seq = seq++, Id = "Copper" });
            games[name].Execute("demo", new PlayCardCommand { Seq = seq++, Id = "Copper" });
            games[name].Execute("demo", new PlayCardCommand { Seq = seq++, Id = "Copper" });
            games[name].Execute("demo", new PlayCardCommand { Seq = seq++, Id = "Copper" });
            games[name].Execute("demo", new BuyCardCommand { Seq = seq++, Id = "Moat" });

            games[name].Execute("agatha", new SetNextPlayerCommand { Seq = seq++, Player = "demo" });
            games[name].Execute("agatha", new PlayCardCommand { Seq = seq++, Id = "Copper" });
            games[name].Execute("agatha", new PlayCardCommand { Seq = seq++, Id = "Copper" });
            games[name].Execute("agatha", new PlayCardCommand { Seq = seq++, Id = "Copper" });
            games[name].Execute("agatha", new PlayCardCommand { Seq = seq++, Id = "Copper" });
            games[name].Execute("agatha", new BuyCardCommand { Seq = seq++, Id = "Militia" });
            
            // games[name].Execute("demo", new SetNextPlayerCommand { Seq = seq++, Player = "agatha" });
            // games[name].Execute("demo", new EndTurnCommand { Seq = seq++ });
            
            // games[name].Execute("agatha", new EndTurnCommand { Seq = seq++ });
            // games[name].Execute("agatha", new PlayCardCommand { Seq = seq++, Id = "Militia" });
        }

        // clean up finished games
        private void OnGameEnded(string name)
        {
            Task.Delay(TimeSpan.FromMinutes(5)).ContinueWith(t => 
            {
                games.Remove(name);
            });
        }
    }
}