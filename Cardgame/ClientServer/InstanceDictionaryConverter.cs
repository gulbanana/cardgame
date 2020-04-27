using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cardgame.Model;

namespace Cardgame.ClientServer
{
    public class InstanceDictionaryConverter<T> : JsonConverter<Dictionary<Instance, T>>
    {
        public override Dictionary<Instance, T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = JsonSerializer.Deserialize<Dictionary<string, T>>(ref reader, options);
            return value.ToDictionary(kvp => Instance.Parse(kvp.Key), kvp => kvp.Value);
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<Instance, T> value, JsonSerializerOptions options)
        {
            var proxyValue = value.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);
            JsonSerializer.Serialize<Dictionary<string, T>>(writer, proxyValue, options);
        }
    }
}