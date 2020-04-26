using Cardgame.Model;

namespace Cardgame.UI
{
    // theoretical client-server separation point
    public interface IGameEndpoint : IEndpoint<GameModel>
    {
        /// <returns>error message</return>
        string Execute(string username, ClientCommand command);
    }
}