namespace Cardgame
{
    public abstract class ClientCommand { }

    public class JoinGameCommand : ClientCommand { }
    public class LeaveGameCommand : ClientCommand { }

    public class ChatCommand : ClientCommand 
    { 
        public string Message { get; set; }
    }
}