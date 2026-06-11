using System;
using Configurations.Model.Types;
using Newtonsoft.Json;

namespace Configurations.Model.Serialization
{
    public sealed class SpriteIdJsonConverter : JsonConverter<SpriteId>
    {
        public override void WriteJson(JsonWriter writer, SpriteId value, JsonSerializer serializer)
        {
            writer.WriteValue(value.id);
        }

        public override SpriteId ReadJson(JsonReader reader, Type objectType, SpriteId existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return reader.TokenType == JsonToken.String ? new SpriteId((string)reader.Value) : SpriteId.Invalid;
        }
    }
}