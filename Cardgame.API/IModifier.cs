namespace Cardgame.API
{
    public interface IModifier
    {
        int ReduceCardCost { get; }
        int IncreaseTreasureValue(string id);
        int? NextHandSize { get; }
    }
}