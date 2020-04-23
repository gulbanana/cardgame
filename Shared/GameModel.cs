using System;
using System.Collections.Generic;
using System.Linq;

namespace Cardgame.Shared
{
    public class GameModel : IModifierSource
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
        public bool KingdomHasCurse { get; set; }
        public bool KingdomHasSeasideMats { get; set; }

        // player config
        public Dictionary<string, bool> SettingConfirmSkipActions { get; set; }
        public Dictionary<string, bool> SettingKeepHandSorted { get; set; }

        // game state
        public string ActivePlayer { get; set; }
        public Dictionary<string, int> Supply { get; set; }
        public Dictionary<string, string[]> SupplyTokens { get; set; }
        public Dictionary<string, List<Instance>> Hands { get; set; }
        public Dictionary<string, List<Instance>> Decks { get; set; }
        public Dictionary<string, List<Instance>> Discards { get; set; }
        public Dictionary<string, List<Instance>> PlayedCards { get; set; }
        public List<Instance> Trash { get; set; }

        // turn state    
        public HashSet<Instance> PreviouslyPlayedCards { get; set; }
        public List<string> ActiveEffects { get; set; }
        public bool ExecutingBackgroundTasks { get; set; }
        public bool BuyPhase { get; set; }
        
        public int ActionsRemaining { get; set; }
        public int BuysRemaining { get; set; }
        public int CoinsRemaining { get; set; }

        // interrupt state
        public HashSet<string> PreventedAttacks { get; set; }
        public Stack<string> ChoosingPlayers { get; set; }
        public string ChoicePrompt { get; set; }
        public ChoiceType ChoiceType { get; set; }
        public string ChoiceInput { get; set; }

        public IModifier[] GetModifiers()
        {
            if (!IsStarted)
            {
                return Array.Empty<IModifier>();
            }
            else
            {
                return ActiveEffects
                    .Select(All.Effects.ByName)
                    .OfType<IModifier>()
                    .ToArray();
            }
        }

        public int GetInitialSupply(string card)
        {
            var victoryCount = Players.Length == 2 ? 8 : 12;
            return card switch
            {
                "Copper" => 60 - (Players.Length * 7),
                "Silver" => 40,
                "Gold" => 30,
                "Curse" => (Players.Length - 1) * 10,
                string id => All.Cards.ByName(id).Types.Contains(API.CardType.Victory) ? victoryCount : 10
            };
        }
    }
}