using System.Collections.Generic;

namespace Cardgame
{
    public class GameModel
    {
        // protection against weird command orderings
        public int Seq { get; set; }

        // metagame state
        public List<TextModel> EventLog { get; } = new List<TextModel>();
        public List<LogEntry> ChatLog { get; } = new List<LogEntry>();        
        public string[] Players { get; set; } = new string[0];
        public bool IsStarted { get; set; }

        // game fixed setup
        public string[] KingdomCards { get; set; } 

        // game state
        public string ActivePlayer { get; set; }
    }
}