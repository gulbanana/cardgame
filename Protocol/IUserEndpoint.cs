namespace Cardgame.Protocol
{
    // theoretical client-server separation point
    public interface IUserEndpoint : IEndpoint<string[]>
    {
        IUserSession FindUser(string username);
    }
}