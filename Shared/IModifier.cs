namespace Cardgame.Shared
{
    public interface IModifier
    {
        int ReduceCardCost { get; }
        int IncreaseTreasureValue(string id);
    }
}