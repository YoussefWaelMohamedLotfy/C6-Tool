using System.Collections.Concurrent;
using C6.Models;
using Microsoft.Extensions.Logging;

namespace C6.Services;

public sealed class LoadCoordinator(
    ILogger<LoadCoordinator> logger,
    MetricsCollector collector,
    IHttpClientFactory httpClientFactory
)
{
    private readonly ConcurrentDictionary<int, (Task Task, CancellationToken Cts)> _activeVus =
        new();
    private int _vuCounter;
    private TestScenario _config;

    public async Task RunTestAsync(TestScenario config, CancellationToken ct)
    {
        _config = config;
        Console.WriteLine($"Starting staged test: {config.TestName}.");

        int currentVuTarget = 0;

        foreach (var stage in config.Stages)
        {
            Console.WriteLine(
                $"\n--- Starting Stage: Target={stage.Target} VUs for {stage.Duration.DurationTimespan.TotalSeconds}s ---"
            );

            var previousVuTarget = currentVuTarget;
            currentVuTarget = stage.Target;

            var rampUpTimeMs = stage.Duration.DurationTimespan.TotalMilliseconds;

            try
            {
                await AdjustLoadAsync(config, previousVuTarget, currentVuTarget, rampUpTimeMs, ct);
            }
            catch (TaskCanceledException ex) when (ct.IsCancellationRequested)
            {
                logger.LogWarning(
                    ex,
                    "Execution cancelled at {CancelledTime}",
                    DateTimeOffset.UtcNow.ToLocalTime()
                );
            }

            await Task.Delay(stage.Duration.DurationTimespan, ct);
        }

        Console.WriteLine("\nAll stages complete. Initiating graceful shutdown...");
        await StopAllVUsAsync(ct);
        Console.WriteLine("Test complete. Processing results...");
    }

    private async Task AdjustLoadAsync(
        TestScenario config,
        int startVUs,
        int endVUs,
        double durationMs,
        CancellationToken ct
    )
    {
        const int stepTimeMs = 500;

        var totalSteps = durationMs / stepTimeMs;
        var vuDeltaPerStep = (endVUs - startVUs) / totalSteps;
        int currentTarget;

        for (int i = 1; i <= totalSteps; i++)
        {
            await Task.Delay(stepTimeMs, ct);
            currentTarget = (int)Math.Round(startVUs + (vuDeltaPerStep * i));
            var vuDifference = currentTarget - _activeVus.Count;

            if (vuDifference > 0)
            {
                for (int j = 0; j < vuDifference; j++)
                {
                    StartNewVu(config, ct);
                }
            }
            else if (vuDifference < 0)
            {
                StopVUs(-vuDifference);
            }

            Console.Write($"\rActive VUs: {_activeVus.Count} / Target: {currentTarget}");
        }
    }

    private void StartNewVu(TestScenario config, CancellationToken ct)
    {
        var vuId = Interlocked.Increment(ref _vuCounter);
        using var vuCts = new CancellationTokenSource();

        // Create the VU instance with the configuration and dependencies
        var vuEngine = new VirtualUser(config, collector, httpClientFactory);

        // Start the VU loop, passing the token
        var vuTask = vuEngine.StartLoopAsync(vuCts.Token);

        // Track the new VU and its cancellation mechanism
        if (!_activeVus.TryAdd(vuId, (vuTask, ct)))
        {
            // Should not happen, but defensive programming
            Console.WriteLine($"[Error] Failed to add VU ID {vuId} to tracking dictionary.");
        }
    }

    private void StopVUs(int count)
    {
        var vusToStop = _activeVus.Take(count).ToList();
        AggregatedResults results = MetricsAggregator.Aggregate(collector.TestResults);
        ConsoleReporter.Report(_config, results);

        foreach (var (vuId, vuData) in vusToStop)
        {
            if (_activeVus.TryRemove(vuId, out _))
            {
                // Optionally, we could await the task here, but it's better
                // to let the main loop wait for overall stage completion.
            }
            else
            {
                Console.WriteLine($"[Warning] Failed to remove VU ID {vuId} for shutdown.");
            }
        }
    }

    private async Task StopAllVUsAsync(CancellationToken ct)
    {
        var tasks = _activeVus.Values.Select(v => v.Task).ToArray();

        try
        {
            await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(10), ct);
        }
        catch (TimeoutException)
        {
            Console.WriteLine("[Warning] Timed out waiting for some VUs to stop gracefully.");
        }
        catch (OperationCanceledException) { }

        _activeVus.Clear();
    }
}
