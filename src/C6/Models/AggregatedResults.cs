namespace C6.Models;

public sealed record AggregatedResults(
    long TotalRequests,
    int ErrorCount,
    double AverageLatencyMs,
    long LatencyP90Ms,
    long LatencyP95Ms,
    long LatencyP99Ms
);
