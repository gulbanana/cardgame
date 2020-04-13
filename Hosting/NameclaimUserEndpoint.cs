namespace Cardgame
{
    // adapter implementing user protocol using DI
    class NameclaimUserEndpoint : IUserEndpoint
    {
        private readonly UserManager manager;

        public NameclaimUserEndpoint(UserManager manager)
        {
            this.manager = manager;
        }

        public IUserSession FindUser(string username)
        {
            return manager.Find(username);
        }
    }
}