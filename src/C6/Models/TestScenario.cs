using System.Text.Json.Serialization;
using C6.Converters;
using Microsoft.AspNetCore.Http;

namespace C6.Models;

public sealed record TestScenario
{
    public string? TestName { get; set; }

    public string TargetUrl { get; set; }

    public C6HttpMethod Method { get; set; }

    public IReadOnlyList<string> Payload { get; set; } = [];

    public int TimeoutMs { get; set; }

    public int SleepMs { get; set; }

    public Stage[] Stages { get; set; }

    [JsonConverter(typeof(HeaderDictionaryJsonConverter))]
    public HeaderDictionary Headers { get; set; }
}

public sealed record Stage
{
    public Duration Duration { get; set; }

    public int Target { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<C6HttpMethod>))]
public enum C6HttpMethod
{
    GET,
    POST,
    PUT,
    DELETE,
    PATCH,
}

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
[JsonSerializable(typeof(TestScenario))]
internal sealed partial class TestScenarioJsonSerializerContext : JsonSerializerContext;
