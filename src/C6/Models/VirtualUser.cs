using System.Diagnostics;
using C6.Services;

namespace C6.Models;

public sealed class VirtualUser(
    TestScenario config,
    MetricsCollector collector,
    IHttpClientFactory httpClientFactory
)
{
    public async Task StartLoopAsync(CancellationToken cancellationToken)
    {
        var targetUri = new Uri(config.TargetUrl);
        HttpMethod method = MapHttpMethod(config.Method);

        while (!cancellationToken.IsCancellationRequested)
        {
            using HttpClient httpClient = httpClientFactory.CreateClient("C6");

            httpClient.Timeout = TimeSpan.FromMilliseconds(config.TimeoutMs);
            using var request = new HttpRequestMessage(method, targetUri);

            // ... (Apply headers, apply optional payload) ...

            var stopwatch = Stopwatch.StartNew();
            long startTime = Stopwatch.GetTimestamp();

            try
            {
                var response = await httpClient.SendAsync(request, cancellationToken);
                stopwatch.Stop();

                var isError = !(response.IsSuccessStatusCode);

                collector.Record(
                    new RequestMetrics(
                        stopwatch.ElapsedMilliseconds, //Stopwatch.GetElapsedTime(startTime).Milliseconds
                        (int)response.StatusCode,
                        isError
                    )
                );
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                collector.Record(new RequestMetrics(stopwatch.ElapsedMilliseconds, 0, true));
            }

            // VU sleeps for the configured time before looping again
            if (config.SleepMs > 0)
            {
                await Task.Delay(config.SleepMs, cancellationToken);
            }
        }
    }

    private static HttpMethod MapHttpMethod(C6HttpMethod method)
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
