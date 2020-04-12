using System.Collections.Generic;

namespace Cardgame
{
    public class GameModel
    {
        public string[] Players { get; set; } = new string[0];
        public List<LogEntry> ChatLog { get; } = new List<LogEntry>();
        public List<TextModel> EventLog { get; } = new List<TextModel>()
        {
            TextModel.Parse("<run>The game began.</run>"),
            TextModel.Parse("<spans><run>You played </run><card>Village</card><run>.</run></spans>"),
            TextModel.Parse("<spans><run>You played </run><card>Market</card><run>.</run></spans>")
        };

        public (string name, string type)[] KingdomCards { get; } = new[]
        {
            ("Estate", "Victory")
        };
    }
}