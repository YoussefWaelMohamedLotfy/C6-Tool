using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Http;

namespace C6.Converters;

public sealed class HeaderDictionaryJsonConverter : JsonConverter<HeaderDictionary>
{
    public override HeaderDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        HeaderDictionary campaignStatuses = [];

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return campaignStatuses;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected PropertyName token.");
            }

            string key = reader.GetString();
            reader.Read();

            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("Expected PropertyName for dictionary key.");
            }

            string value = reader.GetString();
            campaignStatuses.Add(key, value);
        }

        throw new JsonException("Unexpected end of JSON.");
    }

    public override void Write(Utf8JsonWriter writer, HeaderDictionary value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("Store");
        writer.WriteStartObject();

        foreach (var kvp in value)
        {
            writer.WriteString(kvp.Key, kvp.Value);
        }

        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}
