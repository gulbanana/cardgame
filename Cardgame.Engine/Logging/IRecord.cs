namespace Cardgame.Engine.Logging
{
    internal interface IRecord
    {        
        Chunk LatestChunk { get; }
        IRecord CreateSubrecord(string actor, bool inline);
        void Update();
    }
}