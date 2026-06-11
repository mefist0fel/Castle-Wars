using System;
using Configurations.Model.Serialization;
using Configurations.Model.Types;
using Newtonsoft.Json;

/// <summary>
/// Serializes <see cref="ColorData"/> as a compact hex string.
/// Format: "RRGGBB" when A == 255; "RRGGBBAA" otherwise.
/// Auto-registered via <see cref="ConfigurationConverterAttribute"/>.
/// </summary>
[ConfigurationConverter]
public sealed class ColorDataJsonConverter : JsonConverter<ColorData>
{
    public override void WriteJson(JsonWriter writer, ColorData value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override ColorData ReadJson(
        JsonReader reader, Type objectType,
        ColorData existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)   return ColorData.White;
        if (reader.TokenType == JsonToken.String)  return ColorData.FromHex((string)reader.Value);
        return ColorData.White;
    }
}
