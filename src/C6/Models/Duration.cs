using System.Text.Json.Serialization;

using C6.Converters;

namespace C6.Models;

[JsonConverter(typeof(DurationJsonConverter))]
public record struct Duration
{
    public Duration(string value)
    {
        Value = value;
        DurationTimespan = ToTimeSpan();
    }

    public string Value { get; }

    public readonly TimeSpan DurationTimespan { get; }

    public readonly TimeSpan ToTimeSpan()
    {
        if (Value.EndsWith('s') && int.TryParse(Value[..^1], out int seconds))
        {
            return TimeSpan.FromSeconds(seconds);
        }
        else if (Value.EndsWith('m') && int.TryParse(Value[..^1], out int minutes))
        {
            return TimeSpan.FromMinutes(minutes);
        }
        else if (Value.EndsWith('h') && int.TryParse(Value[..^1], out int hours))
        {
            return TimeSpan.FromHours(hours);
        }
        else if (int.TryParse(Value, out int totalSeconds))
        {
            return TimeSpan.FromSeconds(totalSeconds);
        }
        else
        {
            throw new FormatException($"Invalid duration format: {Value}");
        }
    }

    public override readonly string ToString() => Value;
}