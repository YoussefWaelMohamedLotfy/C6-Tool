using System.Collections.Concurrent;
using C6.Models;

namespace C6.Services;

public sealed class MetricsCollector
{
    // ConcurrentQueue is optimized for high-volume, non-blocking producer-consumer scenarios.
    private readonly ConcurrentQueue<RequestMetrics> _metrics = new();

    public void Record(RequestMetrics metric) => _metrics.Enqueue(metric);

    public IReadOnlyList<RequestMetrics> TestResults => [.. _metrics];
}
