using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cardgame.Engine.Logging
{
    // pending https://github.com/dotnet/runtime/issues/779
    internal class ChunkConverter : JsonConverter<Chunk>
    {
        public override Chunk Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Chunk value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            
            if (value.Lines.Any()) 
            {
                writer.WritePropertyName(nameof(Chunk.Lines));
                JsonSerializer.Serialize(writer, value.Lines, options);                    
            }

            if (value.Movements.Any()) 
            {
                writer.WritePropertyName(nameof(Chunk.Movements));
                JsonSerializer.Serialize(writer, value.Movements, options);                    
            }

            if (value.Reshuffled) writer.WriteBoolean(nameof(Chunk.Reshuffled), value.Reshuffled);
            if (value.AddedCards.Any()) 
            {
                writer.WritePropertyName(nameof(Chunk.AddedCards));
                JsonSerializer.Serialize(writer, value.AddedCards, options);                    
            }
            if (value.AddedActions > 0) writer.WriteNumber(nameof(Chunk.AddedActions), value.AddedActions);
            if (value.AddedBuys > 0) writer.WriteNumber(nameof(Chunk.AddedBuys), value.AddedBuys);
            if (value.AddedCoins > 0) writer.WriteNumber(nameof(Chunk.AddedCoins), value.AddedCoins);
            if (value.AddedPotions > 0) writer.WriteNumber(nameof(Chunk.AddedPotions), value.AddedPotions);

            writer.WriteEndObject();
        }
    }
}