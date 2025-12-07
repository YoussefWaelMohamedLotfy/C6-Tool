using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using ConsoleAppFramework;

using Microsoft.Extensions.Logging;

namespace C6.Filters;

internal sealed class LogRunningTimeFilter(ConsoleAppFilter next, ILogger<LogRunningTimeFilter> logger) : ConsoleAppFilter(next)
{
    public override async Task InvokeAsync(ConsoleAppContext context, CancellationToken cancellationToken)
    {
        long startTime = Stopwatch.GetTimestamp();
        logger.LogDebug("Executing command '{CommandName}' at {DateTimeOffsetNowLocalTime}", context.CommandName, DateTimeOffset.UtcNow.ToLocalTime());

        try
        {
            await Next.InvokeAsync(context, cancellationToken);
            logger.LogDebug($"Command '{context.CommandName}' completed successfully at {DateTimeOffset.UtcNow.ToLocalTime()}, Elapsed: {Stopwatch.GetElapsedTime(startTime)}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Command '{context.CommandName}' execution failed at {DateTimeOffset.UtcNow.ToLocalTime()}, Elapsed: {Stopwatch.GetElapsedTime(startTime)}");
            throw;
        }
    }
}