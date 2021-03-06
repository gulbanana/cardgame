namespace Cardgame.Model.ClientServer
{
    // theoretical client-server separation point
    public interface IUserSession
    {
        bool IsLoggedIn { get; }
        string Username { get; }
        bool Login(string username);
        void Logout();
        void CreateBot(string username);
    }
}