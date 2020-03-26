namespace Cardgame
{
    public abstract class ClientCommand { }

    public class JoinGameCommand : ClientCommand { }
    public class LeaveGameCommand : ClientCommand { }
}