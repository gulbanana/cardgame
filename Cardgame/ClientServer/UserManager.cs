using System;
using System.Collections.Generic;
using System.Linq;
using Cardgame.Model.ClientServer;

namespace Cardgame.ClientServer
{
    class UserManager
    {
        private readonly List<IUserSession> sessions;
        public IReadOnlyList<IUserSession> Sessions => sessions;
        public event Action SessionsUpdated;

        public UserManager()
        {
            sessions = new List<IUserSession>();
        }

        public bool Add(IUserSession session)
        {
            var result = false;
            lock (sessions)
            {
                if (sessions.Select(s => s.Username).Contains(session.Username))
                {
                    result = false;
                }
                else
                {
                    sessions.Add(session);
                    result = true;
                }
            }

            if (result)
            {
                SessionsUpdated?.Invoke();
            }

            return result;
        }

        public bool Remove(IUserSession session)
        {
            var result = sessions.Remove(session);

            if (result)
            {
                SessionsUpdated?.Invoke();
            }

            return result;
        }
    }
}