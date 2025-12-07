using System.Text.Json.Serialization;

namespace C6.Models;

public sealed record TestScenario
{
    public string? TestName { get; set; }
    
    public string TargetUrl { get; set; }
    
    public HttpMethod Method { get; set; }
    
    public object[] Payload { get; set; }
    
    public int TimeoutMs { get; set; }
    
    public int SleepMs { get; set; }
    
    public Stage[] Stages { get; set; }
    
    public Dictionary<string, string> Headers { get; set; }
}

public sealed record Stage
{
    public string Duration { get; set; }
    
    public int Target { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<HttpMethod>))]
public enum HttpMethod
{
    GET,
    POST,
    PUT,
    DELETE,
    PATCH
}

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(TestScenario))]
internal sealed partial class TestScenarioJsonSerializerContext : JsonSerializerContext;
