using System;
using System.Collections.Generic;
using System.Linq;

namespace Cardgame
{
    class UserManager
    {
        private readonly List<ScopedUserSession> sessions;
        public IReadOnlyList<ScopedUserSession> Sessions => sessions;

        public UserManager()
        {
            sessions = new List<ScopedUserSession>();
        }

        public bool Add(ScopedUserSession session)
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

        public bool Remove(ScopedUserSession session)
        {
            return sessions.Remove(session);
        }
    }
}