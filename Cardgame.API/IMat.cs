namespace Cardgame.API
{
    public interface IMat
    {
        string Name { get; }
        string Label { get; }
        string Art { get; }
        string GetContents(string[] cards, bool isOwnerOrSpectator);
    }
}