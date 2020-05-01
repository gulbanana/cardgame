namespace Cardgame.Engine.Logging
{
    internal abstract class Event { }

    internal class BeginTurn : Event 
    { 
        public int TurnNumber { get; set; } 
        public string Controller { get; set; }
    }
    
    internal class BuyCard : Event 
    { 
        public string Card { get; set; } 
    }

    internal class PlayCards : Event 
    { 
        public string[] Cards { get; set; } 
    }

    internal class Cleanup : Event { }
}