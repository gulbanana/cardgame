namespace Cardgame
{
    // theoretical client-server separation point
    public interface IUserEndpoint
    {
        IUserSession FindUser(string username);
    }
}