using System;

namespace Cardgame
{
    // adapter implementing user protocol using DI
    class ScopedUserSession : IUserSession, IDisposable
    {
        private readonly UserManager manager;
        public string Username { get; private set; }
        public bool IsLoggedIn => Username != null;

        public ScopedUserSession(UserManager manager)
        {
            this.manager = manager;
        }

        public bool Login(string username)
        {
            Username = username;
            return manager.Add(this);
        }

        public void Logout()
        {
            manager.Remove(this);
        }

        public void Dispose() => Logout();
    }
}