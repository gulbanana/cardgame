using System;
using System.Collections.Generic;
using System.Linq;

namespace Cardgame
{
    class UserSession : IDisposable
    {
        private readonly UserManager manager;
        public string Username { get; private set; }
        public bool IsLoggedIn => Username != null;

        public UserSession(UserManager manager)
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