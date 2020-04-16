namespace Cardgame
{
    // theoretical client-server separation point
    public interface IGameEndpoint : IEndpoint<GameSummary[]>
    {
        IGame FindGame(string name);
    }
}