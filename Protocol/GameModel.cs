using System.Collections.Generic;

namespace Cardgame
{
    public class GameModel
    {
        public string[] Players { get; set; } = new string[0];
        public List<LogEntry> ChatLog { get; } = new List<LogEntry>();

        public (string name, string type)[] KingdomCards { get; } = new[]
        {
            ("Estate", "Victory")
        };
    }
}