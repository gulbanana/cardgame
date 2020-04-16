using Cardgame.Shared;

namespace Cardgame.Client
{
    // theoretical client-server separation point
    public interface IGame : IEndpoint<GameModel>
    {
        /// <returns>error message</return>
        string Execute(string username, ClientCommand command);
    }
}