using System.Collections.Generic;

namespace Cardgame
{
    public class GameModel
    {
        // protection against weird command orderings
        public int Seq { get; set; }

        // metagame state
        public List<string> EventLog { get; set; } 
        public List<LogEntry> ChatLog { get; set; } 
        public string[] Players { get; set; } 
        public bool IsStarted { get; set; }

        // game fixed setup
        public bool IsDemo { get; set; }
        public string[] KingdomCards { get; set; } 
        public Dictionary<string, int> CardStacks { get; set; }

        // game state
        public string ActivePlayer { get; set; }
        public Dictionary<string, List<string>> Hands { get; set; }
        public Dictionary<string, List<string>> Decks { get; set; }
        public Dictionary<string, List<string>> Discards { get; set; }
        public List<string> Trash { get; set; }

        // turn state
        public bool IsExecutingAction { get; set; }
        public bool BuyPhase { get; set; }
        public List<string> PlayedCards { get; set; }        
        public int ActionsRemaining { get; set; }
        public int BuysRemaining { get; set; }
        public int MoneyRemaining { get; set; }

        // interrupt state
        public Stack<string> ChoosingPlayers { get; set; }
        public string ChoicePrompt { get; set; }
        public ChoiceType ChoiceType { get; set; }
        public string ChoiceInput { get; set; }
    }
}