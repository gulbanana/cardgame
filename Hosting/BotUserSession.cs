using Cardgame.Protocol;

namespace Cardgame.Hosting
{
    public class BotUserSession : IUserSession
    {
        public bool IsLoggedIn => true;
        public string Username { get; }

        public BotUserSession(string username)
        {
            Username = username;
        }

        public void CreateBot(string username)
        {
            throw new System.NotImplementedException();
        }

        public bool Login(string username)
        {
            throw new System.NotImplementedException();
        }

        public void Logout()
        {
            throw new System.NotImplementedException();
        }
    }
}