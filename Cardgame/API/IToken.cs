namespace Cardgame.API
{
    public interface IToken : IEffect
    {
        string Description { get; }
    }
}