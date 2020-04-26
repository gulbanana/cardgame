namespace Cardgame.UI
{
    // theoretical client-server separation point
    public interface ILobbyEndpoint : IEndpoint<GameSummary[]>
    {
        IGameEndpoint FindGame(string name);
    }
}