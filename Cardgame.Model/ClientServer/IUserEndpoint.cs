namespace Cardgame.Model.ClientServer
{
    // theoretical client-server separation point
    public interface IUserEndpoint : IEndpoint<string[]>
    {
        IUserSession FindUser(string username);
    }
}