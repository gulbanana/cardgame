using System;
using System.Collections.Generic;
using System.Linq;
using Cardgame.Model.ClientServer;

namespace Cardgame.ClientServer
{
    // adapter implementing user protocol using DI
    class NameclaimUserEndpoint : EndpointBase<string[]>, IUserEndpoint
    {
        private readonly List<Action<string[]>> subscriptions;
        private readonly UserManager manager;

        public NameclaimUserEndpoint(UserManager manager)
        {
            subscriptions = new List<Action<string[]>>();
            this.manager = manager;
            manager.SessionsUpdated += Notify;
        }

        protected override string[] GetModel() 
        {
            return manager.Sessions.Select(s => s.Username).ToArray();
        }

        public IUserSession FindUser(string username)
        {
            return manager.Sessions.Where(s => s.Username.Equals(username)).SingleOrDefault();
        }
    }
}