using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cardgame.Engine.Logging
{
    internal class EventConverter : JsonConverter<Event>
    {
        public override Event Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Event value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.GetType().Name);
            switch (value)
            {
                case BeginTurn { TurnNumber: int turnNumber }:
                    writer.WriteNumber(nameof(BeginTurn.TurnNumber), turnNumber);
                    break;

                case BuyCard { Card: var card }:
                    writer.WriteString(nameof(BuyCard.Card), card);
                    break;

                case PlayCards { Cards: var cards }:
                    writer.WritePropertyName(nameof(PlayCards.Cards));
                    writer.WriteStartArray();
                    foreach (var card in cards)
                    {
                        writer.WriteStringValue(card);
                    }
                    writer.WriteEndArray();
                    break;
            }
        }
    }
}