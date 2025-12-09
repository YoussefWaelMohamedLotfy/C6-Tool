using C6.Models;
using C6.Services;
using ConsoleAppFramework;

namespace C6.Commands;

internal sealed class C6Commands
{
    /// <summary>
    /// Run a simple test based on params
    /// </summary>
    /// <param name="connections">-c, Number of Concurrent Connections</param>
    /// <param name="numberOfRequests">-n, Number of Requests</param>
    /// <param name="url">-u, URL of Server Endpoint</param>
    /// <param name="filePath">-f, Path of File</param>
    /// <param name="ct"></param>
    public void Run(
        [Argument] string url,
        int connections = 0,
        int numberOfRequests = 0,
        string filePath = "",
        CancellationToken ct = default
    )
    {
        Console.WriteLine(
            $"{connections} - {numberOfRequests} - {url} - {(string.IsNullOrEmpty(filePath) ? "No path" : "filepath")}"
        );
    }

    /// <summary>
    /// Run a test based on JSON config path
    /// </summary>
    /// <param name="filePath">-f, Path of File</param>
    /// <param name="ct"></param>
    public async Task RunFile(
        [Argument] string filePath,
        [FromServices] ConfigurationLoader configLoader,
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromServices] LoadCoordinator coordinator,
        [FromServices] MetricsCollector collector,
        CancellationToken ct
    )
    {
        using HttpClient httpClient = httpClientFactory.CreateClient("C6");

        TestScenario scenario = await configLoader.LoadAsync(filePath, ct);
        await coordinator.RunTestAsync(scenario);
        AggregatedResults results = MetricsAggregator.Aggregate(collector.TestResults);
        ConsoleReporter.Report(scenario, results);
    }
}
