namespace Cardgame.Model
{
    public enum Phase
    {
        Action,
        Treasure, // RAW, this is "the start of the Buy phase"
        Buy,
        Cleanup,
    }
}