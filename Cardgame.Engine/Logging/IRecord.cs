using System.Collections.Generic;

namespace Cardgame.Engine.Logging
{
    public interface IRecord
    {        
        List<string> LatestChunk { get; }
        IRecord CreateSubrecord();
        void Update();
    }
}