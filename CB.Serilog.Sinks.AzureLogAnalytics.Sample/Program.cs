// See https://aka.ms/new-console-template for more information

using CB.Serilog.Sinks.AzureLogAnalytics;
using CB.Serilog.Sinks.AzureLogAnalytics.Sample;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Debugging;
using Serilog.Settings.Configuration;



var host = Host.CreateDefaultBuilder()
    .ConfigureHostConfiguration(builder =>
    {

    })
    .ConfigureAppConfiguration((hostContext, builder) =>
    {

    })
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<TestLoggingService>();

    })
    .UseSerilog((hostingContext, services, loggerConfiguration) =>
    {
        SelfLog.Enable(msg => Console.WriteLine(msg));

        var configurationAssemblies = new[]
        {
            typeof(AzureLogAnalyticsSink).Assembly,
            typeof(Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme).Assembly
        };
        var options = new ConfigurationReaderOptions(configurationAssemblies);
        loggerConfiguration
        .Enrich.FromLogContext()        
        .ReadFrom.Services(services)
        .ReadFrom.Configuration(hostingContext.Configuration, options);
    });
await host.RunConsoleAsync();
