using System;
using System.Collections.Generic;
using System.Linq;

namespace Cardgame.Engine.Logging
{
    internal class Subrecord : IRecord
    {
        public readonly string Actor;
        public readonly bool Inline;
        private readonly Action updateThis;
        public readonly List<Section> Sections;

        public Subrecord(string actor, bool inline, Action updateThis)
        {
            this.Actor = actor;
            this.Inline = inline;
            this.updateThis = updateThis;
            Sections = new List<Section>();
        }

        protected Subrecord(string actor)
        {
            this.Actor = actor;
            this.Inline = true;
            this.updateThis = null;
            Sections = new List<Section>();
        }
                
        public bool HasContent()
        {
            return Sections.Any(s => s.Chunk != null && s.Chunk.HasContent() || s.Subrecord.HasContent());
        }
        
        public Chunk LatestChunk
        {
            get
            {
                if (!Sections.Any())
                {
                    var firstSection = new Section(Actor);
                    Sections.Add(firstSection);
                    return firstSection.Chunk;
                }
                else if (Sections.Last().Chunk != null)
                {
                    return Sections.Last().Chunk;
                }
                else
                {
                    var newSection = new Section(Actor);
                    Sections.Add(newSection);
                    return newSection.Chunk;
                }
            }
        }

        public IRecord CreateSubrecord(string actor, bool inline)
        {
            var newRecord = new Subrecord(actor, inline, Update);
            var newSection = new Section(newRecord);
            Sections.Add(newSection);
            return newRecord;
        }

        public virtual void Update() => updateThis();
    }
}