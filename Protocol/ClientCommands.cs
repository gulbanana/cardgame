namespace Cardgame
{
    public abstract class ClientCommand 
    { 
        public int Seq { get; set; }
    }

    public class JoinGameCommand : ClientCommand { }
    public class LeaveGameCommand : ClientCommand { }
    public class StartGameCommand : ClientCommand { }

    public class ChatCommand : ClientCommand 
    { 
        public string Message { get; set; }
    }

    public class PlayCardCommand : ClientCommand
    {
        public string Id { get; set; }
    }

    public class BuyCardCommand : ClientCommand
    {
        public string Id { get; set; }
    }

    public class EndTurnCommand : ClientCommand {}
}