using System;

namespace Cardgame
{
    // theoretical client-server separation point
    public interface IUserEndpoint
    {
        IUserSession FindUser(string username);
        
        string[] Subscribe(Action<string[]> update);
        void Unsubscribe(Action<string[]> update);
    }
}