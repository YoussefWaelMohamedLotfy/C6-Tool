using System.Collections.Concurrent;
using C6.Models;

namespace C6.Services;

public sealed class LoadCoordinator(
    MetricsCollector collector,
    IHttpClientFactory httpClientFactory
)
{
    private readonly ConcurrentDictionary<
        int,
        (Task Task, CancellationTokenSource Cts)
    > _activeVus = new();
    private int _vuCounter;
    private TestScenario _config;

    public async Task RunTestAsync(TestScenario config)
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

            // Start the monitoring and load adjustment task for this stage
            await AdjustLoadAsync(config, previousVuTarget, currentVuTarget, rampUpTimeMs);

            // The coordinator now waits for the full stage duration
            await Task.Delay(stage.Duration.DurationTimespan);
        }

        Console.WriteLine("\nAll stages complete. Initiating graceful shutdown...");
        await StopAllVUsAsync();
        Console.WriteLine("Test complete. Processing results...");
    }

    private async Task AdjustLoadAsync(
        TestScenario config,
        int startVUs,
        int endVUs,
        double durationMs
    )
    {
        // Time window for each adjustment step (e.g., adjust every 500ms)
        const int stepTimeMs = 500;

        var totalSteps = durationMs / stepTimeMs;
        var vuDeltaPerStep = (endVUs - startVUs) / totalSteps;
        int currentTarget;

        for (int i = 1; i <= totalSteps; i++)
        {
            await Task.Delay(stepTimeMs);
            currentTarget = (int)Math.Round(startVUs + (vuDeltaPerStep * i));

            var vuDifference = currentTarget - _activeVus.Count;

            if (vuDifference > 0)
            {
                // Ramp Up: Start new VUs
                for (int j = 0; j < vuDifference; j++)
                {
                    StartNewVu(config);
                }
            }
            else if (vuDifference < 0)
            {
                // Ramp Down: Stop extra VUs (requires cancellation logic)
                StopVUs(-vuDifference);
            }

            Console.Write($"\rActive VUs: {_activeVus.Count} / Target: {currentTarget}");
        }
    }

    private void StartNewVu(TestScenario config)
    {
        var vuId = Interlocked.Increment(ref _vuCounter);
        using var vuCts = new CancellationTokenSource();

        // Create the VU instance with the configuration and dependencies
        var vuEngine = new VirtualUser(config, collector, httpClientFactory);

        // Start the VU loop, passing the token
        var vuTask = vuEngine.StartLoopAsync(vuCts.Token);

        // Track the new VU and its cancellation mechanism
        if (!_activeVus.TryAdd(vuId, (vuTask, vuCts)))
        {
            // Should not happen, but defensive programming
            Console.WriteLine($"[Error] Failed to add VU ID {vuId} to tracking dictionary.");
        }
    }

    private void StopVUs(int count)
    {
        // Simple logic: Stop the VUs that started first (oldest ones).
        var vusToStop = _activeVus.Take(count).ToList();

        foreach (var (vuId, vuData) in vusToStop)
        {
            // 1. Signal the cancellation token
            vuData.Cts.Cancel();

            // 2. Remove the VU from the active list immediately
            // (The task will naturally exit its loop due to the cancellation token)
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

    private async Task StopAllVUsAsync()
    {
        // Signal cancellation for every running VU
        foreach (var vuData in _activeVus.Values)
        {
            await vuData.Cts.CancelAsync();
        }

        // Await all tasks to finish their final request and exit their loops
        var tasks = _activeVus.Values.Select(v => v.Task).ToArray();

        try
        {
            // Use a timeout in case a VU is stuck, prevents the whole CLI from hanging
            await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(10));
        }
        catch (TimeoutException)
        {
            Console.WriteLine("[Warning] Timed out waiting for some VUs to stop gracefully.");
        }
        catch (OperationCanceledException)
        {
            // Expected when waiting on cancelled tasks
            AggregatedResults results = MetricsAggregator.Aggregate(collector.TestResults);
            ConsoleReporter.Report(_config, results);
        }

        _activeVus.Clear();
    }
}
