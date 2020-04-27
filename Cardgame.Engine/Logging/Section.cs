using System.Text.Json.Serialization;

namespace Cardgame.Engine.Logging
{
    internal class Section
    {
        [JsonPropertyName("Record")]
        public Chunk Chunk { get; }
        public Subrecord Subrecord { get; }

        public Section(string player)
        {
            Chunk = new Chunk(player);
        }

        public Section(Subrecord subrecord)
        {
            Subrecord = subrecord;
        }
    }
}