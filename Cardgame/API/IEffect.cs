namespace Cardgame.API
{
    public interface IEffect
    {
        string Name { get; }
        string Art { get; }
        string Text { get; }
    }
}