using C6.Commands;
using C6.Filters;

using ConsoleAppFramework;

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
    .ConfigureServices(service =>
    {
    });

app.UseFilter<LogRunningTimeFilter>();
app.Add<C6Commands>();
await app.RunAsync(args);
