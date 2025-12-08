using System.Diagnostics;
using C6.Services;
using Microsoft.AspNetCore.Http;

namespace C6.Models;

public sealed class VirtualUser(
    TestScenario _config,
    MetricsCollector _collector,
    HttpClient _httpClient
)
{
    // ... (Fields and Constructor) ...

    // Now accepts a CancellationToken to allow the Coordinator to stop it.
    public async Task StartLoopAsync(CancellationToken cancellationToken)
    {
        var targetUri = new Uri(_config.TargetUrl);
        HttpMethod method = MapHttpMethod(_config.Method);

        // Set up the timeout for this individual VU
        _httpClient.Timeout = TimeSpan.FromMilliseconds(_config.TimeoutMs);

        while (!cancellationToken.IsCancellationRequested)
        {
            var request = new HttpRequestMessage(method, targetUri);

            // ... (Apply headers, apply optional payload) ...

            var stopwatch = Stopwatch.StartNew();
            long startTime = Stopwatch.GetTimestamp();

            try
            {
                var response = await _httpClient.SendAsync(request, cancellationToken);
                stopwatch.Stop();

                var isError = !(response.IsSuccessStatusCode);

                // Record the result
                _collector.Record(
                    new RequestMetrics(
                        stopwatch.ElapsedMilliseconds, //Stopwatch.GetElapsedTime(startTime).Milliseconds
                        (int)response.StatusCode,
                        isError
                    )
                );
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // The coordinator requested shutdown, exit loop gracefully.
                return;
            }
            catch (Exception ex)
            {
                // Handle network errors, timeouts, etc.
                stopwatch.Stop();
                _collector.Record(new RequestMetrics(stopwatch.ElapsedMilliseconds, 0, true));
            }

            // VU sleeps for the configured time before looping again
            if (_config.SleepMs > 0)
            {
                await Task.Delay(_config.SleepMs, cancellationToken);
            }
        }
    }

    private HttpMethod MapHttpMethod(C6HttpMethod method)
    {
        return method switch
        {
            C6HttpMethod.GET => HttpMethod.Get,
            C6HttpMethod.POST => HttpMethod.Post,
            C6HttpMethod.PUT => HttpMethod.Put,
            C6HttpMethod.DELETE => HttpMethod.Delete,
            C6HttpMethod.PATCH => HttpMethod.Patch,
            _ => throw new NotSupportedException($"HTTP method {method} is not supported."),
        };
    }
}
