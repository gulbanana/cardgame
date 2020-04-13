namespace Cardgame
{
    // theoretical client-server separation point
    public interface IGameEndpoint
    {
        IGame FindGame(string name);
        string[] ListGames();
    }
}