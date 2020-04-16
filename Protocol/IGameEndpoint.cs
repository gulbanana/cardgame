namespace Cardgame.Protocol
{
    // theoretical client-server separation point
    public interface IGameEndpoint : IEndpoint<GameSummary[]>
    {
        IGame FindGame(string name);
    }
}