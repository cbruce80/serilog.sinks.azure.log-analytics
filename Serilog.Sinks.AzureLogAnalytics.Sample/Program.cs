// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.AzureLogAnalytics;
using Serilog.Sinks.AzureLogAnalytics.Sample;
using System.Reflection;

var host = Host.CreateDefaultBuilder()
    .ConfigureHostConfiguration(builder =>
    {
        builder.AddUserSecrets(Assembly.GetExecutingAssembly());
    })
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<TestLoggingService>();

    })
    .UseSerilog((hostingContext, services, loggerConfiguration) => loggerConfiguration
        .Enrich.FromLogContext()
        .ReadFrom.Services(services)
        .WriteTo.Console()
        .WriteTo.AzureLogAnalytics(() =>
        {
            var configSection = hostingContext.Configuration.GetSection("SerilogLogAnalyticsSink");
            var config = configSection.Get<AzureLogAnalyticsSinkConfiguration>();
            if (config == null)
            {
                throw new NullReferenceException("SerilogLogAnalyticsSink section is missing");
            }
            return config;
        }));

await host.RunConsoleAsync();
