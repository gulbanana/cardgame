using System;
using System.Collections.Generic;
using Cardgame.Client;
using Cardgame.Shared;
using Cardgame.Server;

namespace Cardgame.Hosting
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

            engine.ActionUpdated += OnActionUpdated;

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
                
                Notify();

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

        private void OnActionUpdated()
        {
            Notify();
        }

        private void UpdateSummary()
        {
            Summary = new GameSummary
            {
                Name = name,
                Players = engine.Model.Players,
                Status = engine.Model.IsFinished ? "finished"
                    : engine.Model.IsStarted ? "in progress" 
                    : "waiting to start"
            };            
            
            SummaryUpdated?.Invoke();
        }
    }
}