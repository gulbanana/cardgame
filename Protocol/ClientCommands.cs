namespace Cardgame
{
    public abstract class ClientCommand 
    { 
        public int Seq { get; set; }
    }

    public class SetDemoCommand : ClientCommand { }
    public class SetNextPlayerCommand : ClientCommand 
    { 
        public string Player { get; set; }
    }

    public class JoinGameCommand : ClientCommand
    {
        public bool IsBot { get; set; }
    }
    public class LeaveGameCommand : ClientCommand { }
    public class StartGameCommand : ClientCommand 
    { 
        public CardSet KingdomSet { get; set; }
    }

    public class ChatCommand : ClientCommand 
    { 
        public string Message { get; set; }
    }

    public class PlayCardCommand : ClientCommand
    {
        public string Id { get; set; }
    }

    public class PlayAllTreasuresCommand : ClientCommand { }

    public class BuyCardCommand : ClientCommand
    {
        public string Id { get; set; }
    }

    public class EnterChoiceCommand : ClientCommand 
    {
        public string Output { get; set; }
    }

    public class EndTurnCommand : ClientCommand {}
}