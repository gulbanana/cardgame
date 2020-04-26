using Cardgame.Model;

namespace Cardgame.Model.ClientServer
{
    // theoretical client-server separation point
    public interface IGameEndpoint : IEndpoint<GameModel>
    {
        /// <returns>error message</return>
        string Execute(string username, ClientCommand command);
    }
}