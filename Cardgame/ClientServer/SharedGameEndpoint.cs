using System;
using System.Collections.Generic;
using Cardgame.Model;
using Cardgame.Model.ClientServer;
using Cardgame.Engine;

namespace Cardgame.ClientServer
{
    // adapter implementing game protocol using shared memory
    class SharedGame : EndpointBase<GameModel>, IGameEndpoint
    {
        private readonly List<Action<GameModel>> subscriptions;
        private readonly GameEngine engine;
        private readonly string name;
        public GameSummary Summary { get; private set; }
        public event Action SummaryUpdated;
        public event Action<string> GameEnded;

        public SharedGame(string name)
        {
            subscriptions = new List<Action<GameModel>>();
            engine = new GameEngine();
            this.name = name;

            engine.ModelUpdated += OnModelUpdated;

            UpdateSummary();
        }

        protected override GameModel GetModel()
        {
            return engine.Model;
        }

        public string Execute(string username, ClientCommand command)
        {
            try
            {
                engine.Execute(username, command);
                
                if (command is JoinGameCommand || command is LeaveGameCommand || command is StartGameCommand)
                {
                    UpdateSummary();
                }

                if (engine.Model.IsFinished)
                {
                    UpdateSummary();
                    GameEnded?.Invoke(name);
                }

                return null;
            }
            catch (CommandException e)
            {
                return $"{username}: error executing command {command.GetType().Name}: {e.Message}";
            }
        }

        private void OnModelUpdated()
        {
            Notify();
        }

        private void UpdateSummary()
        {
            Summary = new GameSummary(name, engine.Model);
            SummaryUpdated?.Invoke();
        }
    }
}