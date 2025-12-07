using System.Text.Json;

using C6.Models;

using Microsoft.Extensions.Logging;

namespace C6.Services;

public sealed class ConfigurationLoader(ILogger<ConfigurationLoader> logger)
{
    public async Task<TestScenario> LoadAsync(string filePath, CancellationToken ct)
    {
        string json = await File.ReadAllTextAsync(filePath, ct);
        logger.LogTrace("JSON Scenario file '{FilePath}' has been read", filePath);
        var config = JsonSerializer.Deserialize(json, TestScenarioJsonSerializerContext.Default.TestScenario);
        logger.LogTrace("JSON Scenario file '{FilePath}' parsed", filePath);

        return config ?? throw new JsonException("Failed to deserialize configuration file.");
    }
}