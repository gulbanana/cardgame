using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Cardgame.Protocol;
using Microsoft.AspNetCore.Components.Authorization;

namespace Cardgame.Hosting
{
    // adapter implementing user protocol using DI
    class NameclaimUserSession : AuthenticationStateProvider, IUserSession, IDisposable
    {
        private readonly UserManager manager;
        public string Username { get; private set; }
        public bool IsLoggedIn => Username != null;

        public NameclaimUserSession(UserManager manager)
        {
            this.manager = manager;
        }

        public bool Login(string username)
        {
            Username = username;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return manager.Add(this);
        }

        public void Logout()
        {
            Username = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            manager.Remove(this);
        }

        public void CreateBot(string username)
        {
            manager.Add(new BotUserSession(username));
        }

        public void Dispose() => Logout();

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (IsLoggedIn)
            {
                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, Username),
                }, "name-claim");

                var user = new ClaimsPrincipal(identity);

                return Task.FromResult(new AuthenticationState(user));
            }
            else
            {
                var user = new ClaimsPrincipal();
                return Task.FromResult(new AuthenticationState(user));
            }
        }
    }
}