using System.Collections.Generic;

namespace Cardgame
{
    // adapter implementing game protocol using shared memory
    class SharedGameEndpoint : IGameEndpoint
    {
        private readonly Dictionary<string, SharedGame> games;

        public SharedGameEndpoint()
        {
            games = new Dictionary<string, SharedGame>();
        }

        public IGame FindGame(string name)
        {
            if (!games.ContainsKey(name))
            {
                games[name] = new SharedGame();

                if (name.StartsWith("demo"))
                {
                    games[name].Execute("agatha", new JoinGameCommand { Seq = 0 });
                    games[name].Execute("agatha", new ChatCommand { Seq = 1, Message = "Hello, kingdom!" });
                    games[name].Execute("beto", new JoinGameCommand { Seq = 2 });
                    games[name].Execute("cordelia", new JoinGameCommand { Seq = 3 });
                    games[name].Execute("demo", new JoinGameCommand { Seq = 4 });
                    games[name].Execute("edgar", new JoinGameCommand { Seq = 5 });
                    games[name].Execute("edgar", new ChatCommand { Seq = 6, Message = "th̨o̞̞̞͈̦s̵̺̥͉e ̺͉̹̻o̸̰f́ ̸̪͔̖ͅp̧̺͎a̫͚̗͔̯̖͘r̫t͍i͙͉̩̥͕͔͕c̛̩ṳ̮̻͍l̮̗̝̯a҉̩͈͙̗̟̼̼r͚̺̬̗̖̼͍ ͞co͎͙̮n̬̘͇̺͟c̥̞͉e̷͖r̥͟n̴" });
                    games[name].Execute("demo", new StartGameCommand { Seq = 7 });
                    games[name].Execute("agatha", new ChatCommand { Seq = 8, Message = "Welcome, demo! Nice of you to finally join us!" });
                }
            }

            return games[name];
        }
    }
}