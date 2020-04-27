using System.Collections.Generic;

namespace Cardgame.Engine.Logging
{
    internal interface IRecord
    {        
        List<string> LatestChunk { get; }
        IRecord CreateSubrecord();
        void Update();
    }
}