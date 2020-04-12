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

            var demoGame = new GameModel
            {
                EventLog =
                {
                    TextModel.Parse("<run>The game began.</run>"),
                    TextModel.Parse("<spans><player>demo</player><run> played </run><card>Village</card><run>.</run></spans>"),
                    TextModel.Parse("<spans><player>demo</player><run> played </run><card>Market</card><run>.</run></spans>")
                },
                ChatLog = 
                {
                    new LogEntry { Username = "agatha", Message = "Welcome, demo! Nice of you to finally join us!" },
                },
                IsStarted = true,
                KingdomCards = new[]
                {
                    "Cellar", "Market", "Mine", "Remodel", "Moat",
                    "Smithy", "Village", "Woodcutter", "Workshop", "Militia"
                },
                ActivePlayer = "demo"
            };

            games["demo"] = new SharedGame();
            games["demo"].Execute("agatha", new JoinGameCommand { Seq = 0 });
            games["demo"].Execute("agatha", new ChatCommand { Seq = 1, Message = "Hello, kingdom!" });
            games["demo"].Execute("beto", new JoinGameCommand { Seq = 2 });
            games["demo"].Execute("cordelia", new JoinGameCommand { Seq = 3 });
            games["demo"].Execute("demo", new JoinGameCommand { Seq = 4 });
            games["demo"].Execute("edgar", new JoinGameCommand { Seq = 5 });
            games["demo"].Execute("edgar", new ChatCommand { Seq = 6, Message = "th̨o̞̞̞͈̦s̵̺̥͉e ̺͉̹̻o̸̰f́ ̸̪͔̖ͅp̧̺͎a̫͚̗͔̯̖͘r̫t͍i͙͉̩̥͕͔͕c̛̩ṳ̮̻͍l̮̗̝̯a҉̩͈͙̗̟̼̼r͚̺̬̗̖̼͍ ͞co͎͙̮n̬̘͇̺͟c̥̞͉e̷͖r̥͟n̴" });
            games["demo"].Execute("demo", new StartGameCommand { Seq = 7 });
            games["demo"].Execute("demo", new PlayCardCommand { Seq = 8, Id = "Village" });
            games["demo"].Execute("demo", new PlayCardCommand { Seq = 9, Id = "Market" });
            games["demo"].Execute("agatha", new ChatCommand { Seq = 10, Message = "Welcome, demo! Nice of you to finally join us!" });
        }

        public IGame FindGame(string name)
        {
            if (!games.ContainsKey(name))
            {
                games[name] = new SharedGame();
            }

            return games[name];
        }
    }
}