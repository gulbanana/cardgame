namespace Cardgame.Engine.Logging
{
    internal interface IRecord
    {        
        Chunk LatestChunk { get; }
        void CloseChunk();
        IRecord CreateSubrecord(string actor, bool inline);
        void Update();
    }
}