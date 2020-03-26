using System;
using System.Collections.Generic;
using System.Linq;

namespace Cardgame
{
    class UserManager
    {
        private readonly List<UserSession> sessions;
        public IReadOnlyList<UserSession> Sessions => sessions;

        public UserManager()
        {
            sessions = new List<UserSession>();
        }

        public bool Add(UserSession session)
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

        public bool Remove(UserSession session)
        {
            return sessions.Remove(session);
        }
    }
}