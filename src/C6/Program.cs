using C6.Commands;
using C6.Filters;
using C6.Services;

using ConsoleAppFramework;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ZLogger;

ConsoleApp.Version = "1.0.0";
ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create()
    .ConfigureLogging(x =>
    {
        x.ClearProviders();
        x.SetMinimumLevel(
#if DEBUG
            LogLevel.Trace
#else
            LogLevel.Information
#endif
        );
        x.AddZLoggerConsole();
        //x.AddZLoggerFile("log.txt");
    })
    .ConfigureServices(services =>
    {
        services.AddSingleton<ConfigurationLoader>();
    });

app.UseFilter<LogRunningTimeFilter>();
app.Add<C6Commands>();

await app.RunAsync(args);
