using C6.Models;
using Spectre.Console;

namespace C6.Services;

public static class ConsoleReporter
{
    public static void Report(TestScenario config, AggregatedResults results)
    {
        var table = new Table().Title($"[bold green]C#Load Test Results: {config.TestName}[/]");
        table.AddColumns(new TableColumn("Metric").Centered(), new TableColumn("Value").Centered());

        var successRate =
            (double)(results.TotalRequests - results.ErrorCount) / results.TotalRequests * 100;
        var rps =
            (double)results.TotalRequests
            / config.Stages.Sum(x => x.Duration.DurationTimespan.Seconds);

        table.AddRow("[yellow]Virtual Users[/]", $"[bold]{config.Stages.Max(x => x.Target)}[/]");
        table.AddRow(
            "[yellow]Duration (s)[/]",
            $"[bold]{config.Stages.Sum(x => x.Duration.DurationTimespan.Seconds)}[/]"
        );
        table.AddRow("---", "---");
        table.AddRow("[blue]Total Requests[/]", $"{results.TotalRequests:N0}");
        table.AddRow("[red]Errors[/]", $"{results.ErrorCount:N0}");
        table.AddRow("[green]Success Rate[/]", $"{successRate:F2}%");
        table.AddRow("[cyan]Requests/sec (RPS)[/]", $"{rps:F2}");
        table.AddRow("---", "---");
        table.AddRow("[white]Avg. Latency (ms)[/]", $"{results.AverageLatencyMs:F2}");
        table.AddRow("[white]P90 Latency (ms)[/]", $"{results.LatencyP90Ms:N0}");
        table.AddRow("[white]P95 Latency (ms)[/]", $"{results.LatencyP95Ms:N0}");

        AnsiConsole.Write(table);
    }
}
