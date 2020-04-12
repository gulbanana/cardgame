using System;
using System.Collections.Generic;

namespace Cardgame
{
    // adapter implementing game protocol using shared memory
    class SharedGame : IGame
    {
        private readonly List<Action<GameModel>> subscriptions;
        private readonly GameEngine engine;

        public SharedGame()
        {
            subscriptions = new List<Action<GameModel>>();
            engine = new GameEngine();
        }

        public GameModel Subscribe(Action<GameModel> update)
        {
            subscriptions.Add(update);
            return engine.Model;
        }

        public void Unsubscribe(Action<GameModel> update)
        {
            subscriptions.Remove(update);
        }

        public string Execute(string username, ClientCommand command)
        {
            try
            {
                lock (engine)
                {
                    engine.Execute(username, command);
                }

                foreach (var subscriber in subscriptions)
                {
                    subscriber(engine.Model);
                }

                return null;
            }
            catch (CommandException e)
            {
                return $"{username}: error executing command {command.GetType().Name}: {e.Message}";
            }
        }
    }
}