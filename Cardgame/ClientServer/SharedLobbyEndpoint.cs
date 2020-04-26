using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cardgame.Model.ClientServer;

namespace Cardgame.ClientServer
{
    // adapter implementing game protocol using shared memory
    class SharedLobbyEndpoint : EndpointBase<GameSummary[]>, ILobbyEndpoint
    {
        private readonly Dictionary<string, SharedGame> games;

        public SharedLobbyEndpoint()
        {
            games = new Dictionary<string, SharedGame>();
        }

        protected override GameSummary[] GetModel()
        {
            return games.Values.Select(game => game.Summary).ToArray();
        }

        public IGameEndpoint FindGame(string name)
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

        // clean up finished games
        private void OnGameEnded(string name)
        {
            Task.Delay(TimeSpan.FromMinutes(5)).ContinueWith(t => 
            {
                games.Remove(name);
                Notify();
            });
        }
    }
}