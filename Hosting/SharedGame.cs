using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Cardgame.Hosting
{
    // adapter implementing game protocol using shared memory
    class SharedGame : EndpointBase<GameModel>, IGame
    {
        private readonly List<Action<GameModel>> subscriptions;
        private readonly GameEngine engine;
        private readonly string name;
        public GameSummary Summary { get; private set; }
        public event Action SummaryUpdated;

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
            // return engine.Model;
            var serialised = JsonSerializer.Serialize(engine.Model);
            return JsonSerializer.Deserialize<GameModel>(serialised);
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
                Status = engine.Model.IsStarted ? "in progress" : "waiting to start"
            };            
            
            SummaryUpdated?.Invoke();
        }
    }
}