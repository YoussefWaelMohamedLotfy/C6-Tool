namespace C6.Models;

/// <summary>
/// Defines the metrics collected for a single HTTP request.
/// This will be the data structure collected by the ConcurrentQueue.
/// </summary>
public sealed record RequestMetrics(long LatencyMs, int StatusCode, bool IsError);
