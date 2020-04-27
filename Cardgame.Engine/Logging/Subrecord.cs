using System;
using System.Collections.Generic;
using System.Linq;

namespace Cardgame.Engine.Logging
{
    internal class Subrecord : IRecord
    {
        private readonly Action updateThis;
        public readonly List<Section> Sections;

        public Subrecord(Action updateThis)
        {
            this.updateThis = updateThis;
            Sections = new List<Section>();
        }
                
        public bool HasLines()
        {
            return Sections.Any(s => s.Chunk != null && s.Chunk.Any() || s.Subrecord.HasLines());
        }
        
        public List<string> LatestChunk
        {
            get
            {
                if (!Sections.Any())
                {
                    var firstSection = new Section();
                    Sections.Add(firstSection);
                    return firstSection.Chunk;
                }
                else if (Sections.Last().Chunk != null)
                {
                    return Sections.Last().Chunk;
                }
                else
                {
                    var newSection = new Section();
                    Sections.Add(newSection);
                    return newSection.Chunk;
                }
            }
        }

        public IRecord CreateSubrecord()
        {
            var newRecord = new Subrecord(Update);
            var newSection = new Section(newRecord);
            Sections.Add(newSection);
            return newRecord;
        }

        public virtual void Update() => updateThis();
    }
}