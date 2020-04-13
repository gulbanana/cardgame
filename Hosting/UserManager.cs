using System;
using System.Collections.Generic;
using System.Linq;

namespace Cardgame
{
    class UserManager
    {
        private readonly List<NameclaimUserSession> sessions;
        public IReadOnlyList<NameclaimUserSession> Sessions => sessions;
        public event Action SessionsUpdated;

        public UserManager()
        {
            sessions = new List<NameclaimUserSession>();
        }

        public bool Add(NameclaimUserSession session)
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

        public bool Remove(NameclaimUserSession session)
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