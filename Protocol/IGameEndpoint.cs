using System;

namespace Cardgame
{
    // theoretical client-server separation point
    public interface IGameEndpoint
    {
        GameModel Subscribe(Action<GameModel> update);
        void Unsubscribe(Action<GameModel> update);
        
        /// <returns>error message</return>
        string Execute(string username, ClientCommand command);
    }
}