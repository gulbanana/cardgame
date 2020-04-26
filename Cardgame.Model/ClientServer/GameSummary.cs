namespace Cardgame.Model.ClientServer
{
    public class GameSummary
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string[] Players { get; set; }

        public GameSummary(string name, GameModel model)
        {
            Name = name;
            Players = model.Players;
            Status = model.IsFinished ? "finished"
                : model.IsStarted ? "in progress" 
                : "waiting to start";
        }
    }
}