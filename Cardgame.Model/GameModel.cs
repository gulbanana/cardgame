using System;
using System.Collections.Generic;
using System.Linq;

namespace Cardgame.Model
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
        public bool IsFinished { get; set; }

        // game config
        public CardSet KingdomSet { get; set; }
        public string KingdomPreset { get; set; }
        public string[] KingdomCards { get; set; }
        public string[] KingdomGlobalMats { get; set; }
        public string[] KingdomPlayerMats { get; set; }
        public bool KingdomHasCurse { get; set; }
        public bool KingdomHasPotion { get; set; }

        // player config
        public Dictionary<string, bool> SettingConfirmSkipPhases { get; set; }
        public Dictionary<string, bool> SettingKeepHandSorted { get; set; }

        // game state
        public string PreviousPlayer { get; set; }
        public string ActivePlayer { get; set; }
        public Dictionary<string, int> Supply { get; set; }
        public Dictionary<string, string[]> SupplyTokens { get; set; }
        public Dictionary<string, List<Instance>> Hands { get; set; }
        public Dictionary<string, List<Instance>> Decks { get; set; }
        public Dictionary<string, List<Instance>> Discards { get; set; }
        public Dictionary<string, List<Instance>> PlayedCards { get; set; }
        public Dictionary<string, List<Instance>> MatCards { get; set; }
        public Dictionary<string, Dictionary<string, List<Instance>>> PlayerMatCards { get; set; }
        public Dictionary<Instance, Instance> Attachments { get; set; }

        // turn state    
        public HashSet<Instance> PlayedWithDuration { get; set; }
        public HashSet<Instance> PlayedLastTurn { get; set; }
        public List<string> ActiveEffects { get; set; }
        public bool ExecutingBackgroundTasks { get; set; }
        public Phase CurrentPhase { get; set; }
        
        public int ActionsRemaining { get; set; }
        public int BuysRemaining { get; set; }
        public int CoinsRemaining { get; set; }
        public int PotionsRemaining { get; set; }

        // interrupt state
        public HashSet<string> PreventedAttacks { get; set; }
        public Stack<string> ChoosingPlayers { get; set; }
        public string ChoicePrompt { get; set; }
        public ChoiceType ChoiceType { get; set; }
        public string ChoiceInput { get; set; }
    }
}