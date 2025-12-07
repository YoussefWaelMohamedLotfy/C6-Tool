using System.Text.Json;
using System.Text.Json.Serialization;

using C6.Models;

namespace C6.Converters;

public sealed class DurationJsonConverter : JsonConverter<Duration>
{
    public override Duration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Expected string token type for Duration.");
        }

        string stringValue = reader.GetString();
        return new Duration(stringValue);
    }

    public override void Write(Utf8JsonWriter writer, Duration value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
