using System.Collections.Generic;

namespace Cardgame
{
    public class GameModel
    {
        public List<LogEntry> ChatLog { get; } = new List<LogEntry>();
        public List<TextModel> EventLog { get; } = new List<TextModel>();
        public string[] Players { get; set; } = new string[0];
        public bool IsStarted { get; set; }
        public string[] KingdomCards { get; set; } 
    }
}