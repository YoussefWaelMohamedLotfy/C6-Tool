using C6.Models;

namespace C6.Services;

public static class MetricsAggregator
{
    public static AggregatedResults Aggregate(IReadOnlyCollection<RequestMetrics> metrics)
    {
        var latencies = metrics.Where(m => !m.IsError).Select(m => m.LatencyMs).ToList();
        var totalRequests = metrics.Count;
        var totalErrors = metrics.Count(m => m.IsError);

        // Sort for percentile calculation
        latencies.Sort();

        var agg = new AggregatedResults(
            TotalRequests: totalRequests,
            ErrorCount: totalErrors,
            AverageLatencyMs: latencies.DefaultIfEmpty(0).Average(),
            LatencyP90Ms: GetPercentile(latencies, 0.90),
            LatencyP95Ms: GetPercentile(latencies, 0.95),
            LatencyP99Ms: GetPercentile(latencies, 0.99)
        );

        return agg;
    }

    // Helper function for percentile calculation
    private static long GetPercentile(List<long> sortedValues, double percentile)
    {
        if (sortedValues.Count == 0)
            return 0;
        var index = (int)Math.Ceiling(percentile * sortedValues.Count) - 1;
        // Ensure index is within bounds [0, Count - 1]
        index = Math.Max(0, Math.Min(sortedValues.Count - 1, index));
        return sortedValues[index];
    }
}
