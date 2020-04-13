using System;
using System.Collections.Generic;
using System.Linq;

namespace Cardgame
{
    class UserManager
    {
        private readonly List<NameclaimUserSession> sessions;
        public IReadOnlyList<NameclaimUserSession> Sessions => sessions;

        public UserManager()
        {
            sessions = new List<NameclaimUserSession>();
        }

        public bool Add(NameclaimUserSession session)
        {
            lock (sessions)
            {
                if (sessions.Select(s => s.Username).Contains(session.Username))
                {
                    return false;
                }
                else
                {
                    sessions.Add(session);
                    return true;
                }
            }
        }

        public bool Remove(NameclaimUserSession session)
        {
            return sessions.Remove(session);
        }

        public NameclaimUserSession Find(string username)
        {
            return sessions.Where(s => s.Username.Equals(username)).SingleOrDefault();
        }
    }
}