namespace Cardgame.API
{
    public interface IVictoryCard : ICard
    {
        int Score(string[] dominion);
    }
}