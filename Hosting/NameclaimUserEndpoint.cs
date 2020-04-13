using System;
using System.Collections.Generic;
using System.Linq;

namespace Cardgame
{
    // adapter implementing user protocol using DI
    class NameclaimUserEndpoint : IUserEndpoint
    {
        private readonly List<Action<string[]>> subscriptions;
        private readonly UserManager manager;

        public NameclaimUserEndpoint(UserManager manager)
        {
            subscriptions = new List<Action<string[]>>();
            this.manager = manager;
            manager.SessionsUpdated += OnSessionsUpdated;
        }

        public IUserSession FindUser(string username)
        {
            return manager.Sessions.Where(s => s.Username.Equals(username)).SingleOrDefault();
        }

        public string[] Subscribe(Action<string[]> update)
        {
            subscriptions.Add(update);
            return manager.Sessions.Select(s => s.Username).ToArray();
        }

        public void Unsubscribe(Action<string[]> update)
        {
            subscriptions.Remove(update);
        }

        private void OnSessionsUpdated()
        {
            var users = manager.Sessions.Select(s => s.Username).ToArray();
            foreach (var subscriber in subscriptions)
            {
                subscriber(users);
            }
        }
    }
}