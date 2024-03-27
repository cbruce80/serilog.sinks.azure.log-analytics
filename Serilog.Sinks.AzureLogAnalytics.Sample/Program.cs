// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.AzureLogAnalytics.Sample;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddHostedService<TestLoggingService>();
    })
    .UseSerilog((hostingContext, services, loggerConfiguration) => loggerConfiguration
        .Enrich.FromLogContext()
        .WriteTo.Console());

await host.RunConsoleAsync();
