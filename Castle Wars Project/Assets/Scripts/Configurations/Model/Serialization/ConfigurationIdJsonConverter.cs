using System;
using Configurations.Model.Id;
using Newtonsoft.Json;

namespace Configurations.Model.Serialization
{
    public class ConfigurationIdJsonConverter : JsonConverter<ConfigurationId>
    {
        public override void WriteJson(JsonWriter writer, ConfigurationId value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Id);
        }

        public override ConfigurationId ReadJson(JsonReader reader, Type objectType, ConfigurationId existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return reader.TokenType == JsonToken.String ? new ConfigurationId((string)reader.Value) : ConfigurationId.Invalid;
        }
    }
}