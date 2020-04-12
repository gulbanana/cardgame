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
                Players = new[]
                {
                    "agatha", "beto", "cordelia", "demo", "edgar"
                },
                IsStarted = true,
                KingdomCards = new[]
                {
                    "Cellar", "Market", "Mine", "Remodel", "Moat",
                    "Smithy", "Village", "Woodcutter", "Workshop", "Militia"
                },
                ChatLog = 
                {
                    new LogEntry { Username = "agatha", Message = "Hello, kingdom!" },
                    new LogEntry { Username = "edgar", Message = "th̨o̞̞̞͈̦s̵̺̥͉e ̺͉̹̻o̸̰f́ ̸̪͔̖ͅp̧̺͎a̫͚̗͔̯̖͘r̫t͍i͙͉̩̥͕͔͕c̛̩ṳ̮̻͍l̮̗̝̯a҉̩͈͙̗̟̼̼r͚̺̬̗̖̼͍ ͞co͎͙̮n̬̘͇̺͟c̥̞͉e̷͖r̥͟n̴" },
                    new LogEntry { Username = "agatha", Message = "Welcome, demo! Nice of you to finally join us!" },
                },
                EventLog =
                {
                    TextModel.Parse("<run>The game began.</run>"),
                    TextModel.Parse("<spans><run>You played </run><card>Village</card><run>.</run></spans>"),
                    TextModel.Parse("<spans><run>You played </run><card>Market</card><run>.</run></spans>")
                }
            };

            games["demo"] = new SharedGame(demoGame);
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