using System;
using Newtonsoft.Json;

namespace Configurations.Model.Serialization
{
    public sealed class AssetIdJsonConverter : JsonConverter<AssetId>
    {
        public override void WriteJson(JsonWriter writer, AssetId value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.guid);
        }

        public override AssetId ReadJson(JsonReader reader, Type objectType, AssetId existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return reader.TokenType == JsonToken.String ? (AssetId)Activator.CreateInstance(objectType, (string)reader.Value) : null;
        }
    }
}